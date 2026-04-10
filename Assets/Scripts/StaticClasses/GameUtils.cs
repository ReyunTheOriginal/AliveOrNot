using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public static List<Collider2D> SquareHitDetection(Vector2 StartPoint, Vector2 dir, float Range, int AmountOfRays, float DistanceBetweenRays, string TagToCheckFor, LayerMask Layer, bool DebugMode = false){
        Dictionary<int, Collider2D> HitDict = new Dictionary<int, Collider2D>();
        List<Collider2D> List = new List<Collider2D>();

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
            List.Add(hit.Value);

        return List;
    }

    /*public static List<Collider2D> AngledHitDetection(Vector2 StartPoint, Vector2 dir, float Range, int AmountOfRays, float CircleRadius, float AngleDifference,string TagToCheckFor, LayerMask Layer, bool DebugMode = false){
        List<Collider2D> HitList = new List<Collider2D>();

        dir = dir.normalized;

        // center the rays
        float half = (AmountOfRays - 1) * 0.5f;

        for (int i = 0; i < AmountOfRays; i++){

            RaycastHit2D[] hit = Physics2D.RaycastAll(StartPoint, );

            foreach (RaycastHit2D singlehit in hit){
                if (singlehit.collider){
                    HitList.Add(singlehit.collider);
                }
            }

        }

        return HitList;
    }*/

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
    public static void StartIndependentCoroutine(System.Func<IEnumerator> func){
        GameObject TempObject = new GameObject();
        TempObject.name = "Temporary Coroutine Object";
        Temporary TempScript = TempObject.AddComponent<Temporary>();
        TempScript.StartTempCoroutine(func);
    }

    public static Vector2 GetRandomPointInSquareRange(Vector2 CenterPoint, float Range){
        Vector2 result;
        result.x = UnityEngine.Random.Range(-Range, Range);
        result.y = UnityEngine.Random.Range(-Range, Range);

        result += CenterPoint;

        return result;
    }
}
