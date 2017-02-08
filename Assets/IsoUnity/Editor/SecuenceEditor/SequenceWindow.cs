﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Callbacks;

public class SequenceWindow: EditorWindow{


    [OnOpenAsset(1)]
    public static bool Open(int instanceID, int line)
    {
        var o = EditorUtility.InstanceIDToObject(instanceID);
        if(o is SequenceAsset)
        {
            var newWindow = ScriptableObject.CreateInstance<SequenceWindow>();
            newWindow.sequence = o as SequenceAsset;
            newWindow.Show();
            return true;
        }
        return false;
    }

	private Sequence sequence;

	public Sequence Sequence {
		get { return sequence; }
		set { this.sequence = value; }
	}

    private Dictionary<int, SequenceNode> nodes = new Dictionary<int, SequenceNode>();
	private Dictionary<SequenceNode, NodeEditor> editors = new Dictionary<SequenceNode, NodeEditor>();
    private GUIStyle closeStyle, collapseStyle;

    private int hovering = -1;
    private int focusing = -1;

    private int lookingChildSlot;
    private SequenceNode lookingChildNode;
	
	void nodeWindow(int id)
	{
        SequenceNode myNode = nodes[id];

        if (myNode.Collapsed)
        {
            if (GUILayout.Button("Open"))
                myNode.Collapsed = false;
        }
        else
        {

            string[] editorNames = NodeEditorFactory.Intance.CurrentNodeEditors;

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var editorSelected = EditorGUILayout.Popup(NodeEditorFactory.Intance.NodeEditorIndex(myNode), editorNames);

            if(!editors.ContainsKey(myNode) || EditorGUI.EndChangeCheck()){
                var editor = NodeEditorFactory.Intance.createNodeEditorFor(editorNames[editorSelected]);
                editor.useNode(myNode);

                if (!editors.ContainsKey(myNode)) editors.Add(myNode, editor);
                else
                {
                    ScriptableObject.DestroyImmediate(editors[myNode] as Object);
                    editors[myNode] = editor;

                }
            }

            if (GUILayout.Button("-", collapseStyle, GUILayout.Width(15), GUILayout.Height(15)))
                myNode.Collapsed = true;
            if (GUILayout.Button("X", closeStyle, GUILayout.Width(15), GUILayout.Height(15)))
                sequence.removeChild(myNode);

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            editors[myNode].draw();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if(Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                ContextMenu cm = new ContextMenu();
                cm.a
            }

            nodes[id] = editors[myNode].Result;	
        }

		
		
		if (Event.current.type != EventType.layout) {
			Rect lastRect = GUILayoutUtility.GetLastRect ();
            Rect myRect = myNode.Position;
			myRect.height = lastRect.y + lastRect.height;
            myNode.Position = myRect;
			this.Repaint();
        }

        if (Event.current.type == EventType.mouseMove)
        {
            if (new Rect(0, 0, myNode.Position.width, myNode.Position.height).Contains(Event.current.mousePosition))
            {
                hovering = id;
            }
        }

        if (Event.current.type == EventType.mouseDown)
        {
            if (hovering == id) focusing = hovering;
            if (lookingChildNode != null)
            {
                // link creation between nodes
                lookingChildNode.Childs[lookingChildSlot] = myNode;
                // finishing search
                lookingChildNode = null;
            }
        }

		GUI.DragWindow();
	}
	void curveFromTo(Rect wr, Rect wr2, Color color, Color shadow)
	{
        Vector2 start = new Vector2(wr.x + wr.width, wr.y + 3 + wr.height / 2),
            startTangent = new Vector2(wr.x + wr.width + Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr.y + 3 + wr.height / 2),
            end = new Vector2(wr2.x, wr2.y + 3 + wr2.height / 2),
            endTangent = new Vector2(wr2.x - Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr2.y + 3 + wr2.height / 2);

        Handles.BeginGUI();
        Handles.color = color;
        Handles.DrawBezier(start, end, startTangent, endTangent, color, null, 3);
        Handles.EndGUI();

		/*Drawing.bezierLine(
			,
			,
			new Vector2(wr2.x, wr2.y + 3 + wr2.height / 2),
			, shadow, 5, true,20);
		Drawing.bezierLine(
			new Vector2(wr.x + wr.width, wr.y + wr.height / 2),
			new Vector2(wr.x + wr.width + Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr.y + wr.height / 2),
			new Vector2(wr2.x, wr2.y + wr2.height / 2),
			new Vector2(wr2.x - Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr2.y + wr2.height / 2), color, 2, true,20);*/
	}

    private Rect sumRect(Rect r1, Rect r2)
    {
        return new Rect(r1.x + r2.x, r1.y + r2.y, r1.width + r2.width, r1.height + r2.height);
    }

    private Dictionary<SequenceNode, bool> loopCheck = new Dictionary<SequenceNode, bool>();

    void drawLines(Rect from, SequenceNode to)
    {
        if (to == null)
            return;

        // Visible loop line
        curveFromTo(from, to.Position, l, s);



        if (!loopCheck.ContainsKey(to))
        {
            loopCheck.Add(to, true);
            float h = to.Position.height / (to.Childs.Length * 1.0f);
            for (int i = 0; i < to.Childs.Length; i++)
            {        
                Rect fromRect = sumRect(to.Position, new Rect(0, h * i, 0, h-to.Position.height));
                // Looking child line
                if (lookingChildNode == to && i == lookingChildSlot)
                {
                    if (hovering != -1) curveFromTo(fromRect, nodes[hovering].Position, l, s);
                    else curveFromTo(fromRect, new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 1, 1), l, s);
                }
                else drawLines(fromRect, to.Childs[i]);
            }
        }

    }

    void drawLines(Sequence sequence)
    {
        loopCheck.Clear();
        drawLines(new Rect(0, 0, 0, position.height), sequence.Root);

        // Draw the rest of the lines in red
        foreach (var n in sequence.Nodes)
        {
            if (!loopCheck.ContainsKey(n))
            {
                float h = n.Position.height / (n.Childs.Length * 1.0f);
                for (int i = 0; i < n.Childs.Length; i++)
                    if(n.Childs[i]!=null)
                    { 
                        Rect fromRect = sumRect(n.Position, new Rect(0, h * i, 0, h - n.Position.height));
                        curveFromTo(fromRect, n.Childs[i].Position, r, s);
                    }
            }
        }
    }

    /**
     *  Rectangle backup code calculation
     **
     
        if(!rects.ContainsKey(node.Childs[i]))
			rects.Add(node.Childs[i], new Rect(rects[node].x + 315, rects[node].y + i*altura, 150, 0));
		curveFromTo(rects[node], rects[node.Childs[i]], new Color(0.3f,0.7f,0.4f), s);
     
     */
	
	void createWindows(Sequence sequence)
    {
		float altura = 100;
		foreach(var node in sequence.Nodes){
            nodes.Add(node.GetInstanceID(), node);
            node.Position = GUILayout.Window(node.GetInstanceID(), node.Position, nodeWindow, node.Name);
		}
	}
	
	Color s = new Color(0.4f, 0.4f, 0.5f),
        l = new Color(0.3f, 0.7f, 0.4f),
        r = new Color(0.8f, 0.2f, 0.2f);
	void OnGUI()
	{
		if (sequence == null)
			this.Close ();

        this.wantsMouseMove = true;

        if (closeStyle == null)
        {
            closeStyle = new GUIStyle(GUI.skin.button);
            closeStyle.padding = new RectOffset(0, 0, 0, 0);
            closeStyle.margin = new RectOffset(0, 5, 2, 0);
            closeStyle.normal.textColor = Color.red;
            closeStyle.focused.textColor = Color.red;
            closeStyle.active.textColor = Color.red;
            closeStyle.hover.textColor = Color.red;
        }

        if (collapseStyle == null)
        {
            collapseStyle = new GUIStyle(GUI.skin.button);
            collapseStyle.padding = new RectOffset(0, 0, 0, 0);
            collapseStyle.margin = new RectOffset(0, 5, 2, 0);
            collapseStyle.normal.textColor = Color.blue;
            collapseStyle.focused.textColor = Color.blue;
            collapseStyle.active.textColor = Color.blue;
            collapseStyle.hover.textColor = Color.blue;
        }

		SequenceNode nodoInicial = sequence.Root;
        GUILayout.BeginVertical(GUILayout.Height(20));
        GUILayout.BeginHorizontal();

        if(GUILayout.Button("New Node")){
            var node = sequence.createChild();
            node.Position = new Rect(scroll + position.size / 2 - node.Position.size/2, node.Position.size);
            node.Position = new Rect(new Vector2((int)node.Position.x, (int)node.Position.y), node.Position.size);
        }
        if(GUILayout.Button("Set Root")){
            if (nodes.ContainsKey(focusing))
            {
                sequence.Root = nodes[focusing];
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        var lastRect = GUILayoutUtility.GetLastRect();

        var rect = new Rect(0, lastRect.y + lastRect.height, position.width, position.height - lastRect.height);


        float maxX = rect.width, maxY = rect.height;
        foreach(var node in sequence.Nodes)
        {
            var px = node.Position.x + node.Position.width + 50 ;
            var py = node.Position.y + node.Position.height + 50;
            maxX = Mathf.Max(maxX, px);
            maxY = Mathf.Max(maxY, py);
        }
        scrollRect = new Rect(0, 0, maxX, maxY);

        scroll = GUI.BeginScrollView(rect, scroll, scrollRect);
        // Clear mouse hover
        if (Event.current.type == EventType.MouseMove) hovering = -1;

		BeginWindows();
        nodes.Clear();
		createWindows(sequence);

        if (Event.current.type == EventType.Repaint)
            drawLines(sequence);

        EndWindows();
        if(hovering == -1) 
        {
            if(Event.current.type == EventType.MouseDrag)
            {
                scroll -= Event.current.delta;
            }
        }

        GUI.EndScrollView();
	}

    private Rect scrollRect = new Rect(0, 0, 1000, 1000);
    private Vector2 scroll;
}