using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HutongGames.PlayMaker;
using UnityEngine;

// GNU GPL 3.0
#pragma warning disable CS1591
namespace MSCLoader
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FsmHook
    {
        private class FsmHookAction : FsmStateAction
        {
            public Action hook;

            public override void OnEnter()
            {
                hook?.Invoke();
                Finish();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void FsmInject(GameObject gameObject, string stateName, Action hook)
        {
            FsmState state = GetStateFromGameObject(gameObject, stateName);
            if (state != null)
            {
                List<FsmStateAction> actions = new List<FsmStateAction>(state.Actions);
                actions.Insert(0, new FsmHookAction { hook = hook });
                state.Actions = actions.ToArray();
            }
            else ModConsole.LogError(string.Format("Cannot find state <b>{0}</b> in GameObject <b>{1}</b>", stateName, gameObject.name));
        }

        private static FsmState GetStateFromGameObject(GameObject obj, string stateName)
        {
            PlayMakerFSM[] comps = obj.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM playMakerFsm in comps)
            {
                FsmState state = playMakerFsm.FsmStates.FirstOrDefault(x => x.Name == stateName);
                if (state != null) return state;
            }
            return null;
        }
    }
}