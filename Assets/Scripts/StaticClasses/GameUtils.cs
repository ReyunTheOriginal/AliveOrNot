using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class GameUtils{
    public static void MakeObjectLookAt(Transform RotatedObject, Vector2 Target, float Offsit = 0, bool Flip = true){
        Vector2 dir = DirFromAToB(RotatedObject.position, Target);

        if (dir.sqrMagnitude < 0.0001f) return;

        float rotate = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;

        if (Flip && (rotate >=90 || rotate <= -90)){
            RotatedObject.transform.rotation = Quaternion.Euler(0,180,-(rotate + Offsit + 180));
            return;
        }

        RotatedObject.transform.rotation = Quaternion.Euler(0,0,rotate + Offsit);
    }

    public static bool OverASpeedInDirection(Vector2 velocity, Vector2 dir, float speed){
        dir = dir.normalized;
        return Vector2.Dot(velocity, dir) > speed;
    }

    public static Vector2 DirFromAToB(Vector2 A, Vector2 B){
        Vector2 dir = B - A;
        return dir.normalized;
    }

    public static LayerMask LayerMaskFromNumbers(params int[] layers){
        if (layers == null || layers.Length == 0)
            return ~0; // all layers

        bool hasPositive = System.Array.Exists(layers, l => l >= 0);
        int mask = hasPositive ? 0 : ~0; // start from all if only exclusions

        foreach (int layer in layers){
            if (layer >= 0)
                mask |= (1 << layer);       // include
            else
                mask &= ~(1 << -layer);     // exclude
        }
        return mask;
    }

    public static HashSet<Collider2D> SquareHitDetection(Vector2 StartPoint, Vector2 dir, float Range, int AmountOfRays, float DistanceBetweenRays, string TagToCheckFor, LayerMask Layer, bool DebugMode = false){
        Dictionary<int, Collider2D> HitDict = new Dictionary<int, Collider2D>();
        HashSet<Collider2D> HitList = new HashSet<Collider2D>();

        dir = dir.normalized;

        // perpendicular direction
        Vector2 perp = new Vector2(-dir.y, dir.x);

        // center the rays
        float half = (AmountOfRays - 1) * 0.5f;

        for (int i = 0; i < AmountOfRays; i++){
            float offsetAmount = (i - half) * DistanceBetweenRays;
            Vector2 offset = perp * offsetAmount;

            Vector2 origin = StartPoint + offset;

            if (DebugMode)Debug.DrawRay(origin, dir * Range, Color.red);

            RaycastHit2D[] hit = Physics2D.RaycastAll(origin, dir, Range, Layer);

           foreach (RaycastHit2D singlehit in hit)
                if (singlehit.collider && singlehit.collider.CompareTag(TagToCheckFor))
                    HitDict[singlehit.collider.gameObject.GetInstanceID()] = singlehit.collider;

        }

        
        foreach (var hit in HitDict)
            HitList.Add(hit.Value);

        return HitList;
    }

    public static HashSet<Collider2D> AngledHitDetection(Vector2 StartPoint, Vector2 dir, float Range, int AmountOfRays, float AngleDifference,string TagToCheckFor, LayerMask Layer, bool DebugMode = false){
        Dictionary<int, Collider2D> HitDict = new Dictionary<int, Collider2D>();
        HashSet<Collider2D> HitList = new HashSet<Collider2D>();

        dir = dir.normalized;

        // center the rays
        float half = AngleDifference * 0.5f;

        for (int i = 0; i < AmountOfRays; i++){
            // 0 → 1 across all rays
            float t = (AmountOfRays == 1) ? 0.5f : i / (float)(AmountOfRays - 1);

            // angle from -half → +half
            float angle = Mathf.Lerp(-half, half, t);

            // rotate direction
            Vector2 rotatedDir = Quaternion.Euler(0, 0, angle) * dir;

            RaycastHit2D[] hits = Physics2D.RaycastAll(StartPoint, rotatedDir, Range, Layer);

            foreach (RaycastHit2D hit in hits){
                if (hit.collider && hit.collider.CompareTag(TagToCheckFor)){
                    HitDict[hit.collider.gameObject.GetInstanceID()] = hit.collider;
                }
            }

            if (DebugMode)
                Debug.DrawRay(StartPoint, rotatedDir * Range, Color.red);
        }

        foreach (var hit in HitDict)
            HitList.Add(hit.Value);

        return HitList;
    }

    public static void FollowObject(Transform subject, Transform Target, float Speed){
        Vector2 dir = DirFromAToB(subject.transform.position, Target.position);

        subject.position += (Vector3)(dir * Speed);
    }

    public static void FollowObjectWithRig(Rigidbody2D subject, Transform Target, float Speed){
        Vector2 dir = DirFromAToB(subject.transform.position, Target.position);

        if (!OverASpeedInDirection(subject.velocity, dir, Speed)){
            subject.AddForce(dir * Speed, ForceMode2D.Force);
        }
    }

    public static float GetDeterministicRandom(Vector2Int pos, int seed){
        // Pack inputs using large primes to avoid symmetry artifacts
        uint h = (uint)seed;
        h ^= (uint)pos.x * 2246822519u;
        h ^= (uint)pos.y * 3266489917u;
        
        // Murmur3-style finalizer (excellent avalanche effect)
        h ^= h >> 16;
        h *= 2246822519u;
        h ^= h >> 13;
        h *= 3266489917u;
        h ^= h >> 16;

        return h / (float)uint.MaxValue;
    }

    public static Vector2Int MakeIntoVector2Int(Vector2 vec){
        int x = Mathf.FloorToInt(vec.x);
        int y = Mathf.FloorToInt(vec.y);
        return new Vector2Int(x,y);
    } 

    //Use With a Lambda like this for Parameters: "() => MyFunction(type Parameter)"
    public static IndependentCoroutine StartIndependentCoroutine(System.Func<IEnumerator> func){
        GameObject TempObject = new GameObject();
        TempObject.name = "Temporary Coroutine Object";
        Temporary TempScript = TempObject.AddComponent<Temporary>();
        return TempScript.StartTempCoroutine(func);
    }
   
    public static void PlayIndependentAudio(AudioClip Clip, Vector2 Position){
        GameObject TempObject = new GameObject();
        TempObject.transform.position = Position;
        TempObject.name = "Temporary Coroutine Object";
        Temporary TempScript = TempObject.AddComponent<Temporary>();
        TempScript.StartTempAudio(Clip);
    }

    public static Vector2 GetRandomPointInCircleRange(Vector2 center, float maxRadius, float minRadius){
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        float minR2 = minRadius * minRadius;
        float maxR2 = maxRadius * maxRadius;

        float r = Mathf.Sqrt(UnityEngine.Random.Range(minR2, maxR2));

        Vector2 point = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * r;

        return center + point;
    }

    public static Vector2 AddRandomAngleToDir(Vector2 baseDir, float minAngle, float maxAngle){
        float angle = Random.Range(minAngle, maxAngle);
        float rad = angle * Mathf.Deg2Rad;
        
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        
        return new Vector2(
            baseDir.x * cos - baseDir.y * sin,
            baseDir.x * sin + baseDir.y * cos
        );
    }
    
    public static Vector2 ClampVector2(Vector2 subject, float minMag, float maxMag){
        float mag = subject.magnitude;
        if (mag == 0f) return Vector2.zero;
        
        float clamped = Mathf.Clamp(mag, minMag, maxMag);
        return subject.normalized * clamped;
    }
    
    public static Vector2 QuickWorldMousePosition(){
        return GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition);
    }
    public static void ResetCursor(){
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    
    public static void SetCursor(Texture2D Texture, Vector2 HotSpot){
        Cursor.SetCursor(Texture, HotSpot, CursorMode.Auto);
    }

    public class IndependentCoroutine{
        public Coroutine Coroutine;
        public Temporary Owner;
        public void Stop(){
            if (Owner != null && Coroutine != null){
                Owner.StopCoroutine(Coroutine);
                Coroutine = null;
                Object.Destroy(Owner.gameObject);
            }
        }
    }
    public static void RunIndependent(this IEnumerator coroutine){
        StartIndependentCoroutine(() => coroutine);
    }
}
   
