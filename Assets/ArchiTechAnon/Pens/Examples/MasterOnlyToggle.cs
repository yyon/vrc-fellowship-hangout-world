
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MasterOnlyToggle : UdonSharpBehaviour
{
    public GameObject[] toggles;
    private ArchiTech.PickupManager[] pens;
    private bool skipLog;

    private void log(string value)
    {
        if (!skipLog) Debug.Log("[<color=#ccff55>MasterOnlyToggle</color>] " + value);
    }

    void Start()
    {
        var _pens = (VRC_Pickup[])traverseAllFor(typeof(VRC_Pickup), toggles);
        pens = new ArchiTech.PickupManager[_pens.Length];
        var count = 0;
        foreach (var _pen in _pens)
        {
            var pen = _pen.gameObject.GetComponent<ArchiTech.PickupManager>();
            if (pen != null)
            {
                pens[count] = pen;
                count++;
            }
        }
        log("Found " + count + " pens");
    }

    public void OnInteract()
    {
        if (Networking.IsMaster)
        {
            foreach (var pen in pens)
            {
                if (pen != null && pen.HasOwner())
                    pen.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Release");
            }
        }
    }

    // ============== Traversal Utilities ===================

    private Component[] traverseAllFor(System.Type T, GameObject[] objects)
    {
        var elements = new Component[objects.Length];
        var index = 0;
        foreach (GameObject obj in objects)
        {
            if (obj == null) continue;
            Component[] found = traverseFor(T, obj.transform);
            foreach (Component element in found)
            {
                if (element == null) continue;
                // check if next index exceeds bounds, expand if so
                if (index >= elements.Length)
                    elements = expand(elements, 16);
                elements[index] = element;
                index++;
            }
        }
        // remove excess nulls
        var clean = new Component[index];
        for (int i = 0; i < clean.Length; i++)
        {
            clean[i] = elements[i];
        }
        elements = clean;
        log("Total found: " + elements.Length);
        return elements;
    }

    private Component[] traverseFor(System.Type T, Transform root)
    {
        log("Traversing node " + root.name + " for " + T);
        Component[] elements = new Component[16];
        // stack is actually always Transform, but use Component for generic pop/push use
        Component[] stack = new Component[16];
        stack[0] = root;
        var found = 0;
        while (stack[0] != null)
        {
            var cur = (Transform)pop(stack);
            // log("Gathering elements within node " + cur.name);
            if (cur != null)
            {
                var t = cur.GetComponent(T);
                if (t != null)
                {
                    found++;
                    elements = push(elements, t);
                }

                // add all children to stack in reverse order
                for (int i = cur.childCount - 1; i >= 0; i--)
                    stack = push(stack, cur.GetChild(i));
            }
        }
        if (found > 0) log("Found " + found + " matching component(s)");
        return elements;
    }

    private Component[] push(Component[] stack, Component n)
    {
        var added = false;
        for (int i = 0; i < stack.Length; i++)
        {
            if (stack[i] == null)
            {
                stack[i] = n;
                added = true;
                break;
            }
        }
        // unable to add due to stackoverflow, expand stack and retry
        if (!added)
        {
            stack = expand(stack, 16);
            for (int i = 0; i < stack.Length; i++)
            {
                if (stack[i] == null)
                {
                    stack[i] = n;
                    added = true;
                    break;
                }
            }
        }
        return stack;
    }

    private Component pop(Component[] stack)
    {
        // find first non-null entry in the stack, remove and return.
        var last = -1;
        for (int i = 0; i < stack.Length; i++)
        {
            var item = stack[i];
            if (item != null) last = i;
            if (item == null) break;
        }
        if (last == -1) return null;
        var t = stack[last];
        stack[last] = null;
        return t;
    }

    private Component[] expand(Component[] arr, int add)
    {
        // expand beyond size of self if children are found
        var newArray = new Component[arr.Length + add];
        var index = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == null) continue;
            newArray[index] = arr[i];
            index++;
        }
        return newArray;
    }
}
