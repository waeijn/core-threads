using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = null;
    public bool IsPerforming { get; private set; } = false;
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();
    private static Dictionary<System.Delegate, System.Action<GameAction>> delegateWrappers = new();

    /// <summary>
    /// Clears all static dictionaries when entering play mode.
    /// Prevents stale delegates from previous play sessions accumulating 
    /// and causing duplicate reactions (e.g. drawing 10 cards instead of 5).
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        preSubs = new();
        postSubs = new();
        performers = new();
        delegateWrappers = new();
    }

    public void Perform(GameAction action, System.Action OnPerformFinished = null)
    {
        if (IsPerforming) return;
        IsPerforming = true;
        StartCoroutine(Flow(action, () =>
        {
            IsPerforming = false;
            OnPerformFinished?.Invoke();
        }));
    }

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        reactions = action.PreReactions;
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        yield return PerformPerformer(action);
        yield return PerformReactions();

        reactions = action.PostReactions;
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        OnFlowFinished?.Invoke();
    }

    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType();
        if (performers.ContainsKey(type))
        {
            yield return performers[type](action);
        }
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();
        if (subs.ContainsKey(type))
        {
            foreach (var sub in subs[type])
            {
                sub(action);
            }
        }
    }

    private IEnumerator PerformReactions()
    {
        foreach (var reaction in reactions)
        {
            yield return Flow(reaction);
        }
    }

    public void AddReaction(GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }
    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);
        if (performers.ContainsKey(type)) performers[type] = wrappedPerformer;
        else performers.Add(type, wrappedPerformer);
    }

    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type)) performers.Remove(type);
    }

    public static void SubscribeReaction<T>(System.Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        
        System.Action<GameAction> wrappedReaction;
        if (!delegateWrappers.TryGetValue(reaction, out wrappedReaction))
        {
            wrappedReaction = (GameAction action) => reaction((T)action);
            delegateWrappers[reaction] = wrappedReaction;
        }

        if (!subs.ContainsKey(typeof(T)))
        {
            subs.Add(typeof(T), new System.Collections.Generic.List<System.Action<GameAction>>());
        }
        subs[typeof(T)].Add(wrappedReaction);
    }

    public static void UnsubscribeReaction<T>(System.Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        if (subs.ContainsKey(typeof(T)) && delegateWrappers.TryGetValue(reaction, out var wrappedReaction))
        {
            subs[typeof(T)].Remove(wrappedReaction);
        }
    }

}
