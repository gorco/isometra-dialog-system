using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Isometra.Sequences {
	public class SequenceManager : EventManager {
		
		private SequenceInterpreter sequenceInterpreter;
		private Queue<IGameEvent> sequenceQueue = new Queue<IGameEvent>();

		public override void ReceiveEvent (IGameEvent ev)
		{
			if(ev.Name.ToLower() == "start sequence"){
				sequenceQueue.Enqueue(ev);
			}
			else if (ev.Name.ToLower() == "abort sequence")
			{
				sequenceInterpreter = null;
				sequenceQueue.Clear();
			}
			if (sequenceInterpreter != null)
				sequenceInterpreter.EventHappened(ev);
		}

		public override void Tick(){
			if(sequenceQueue.Count > 0 && sequenceInterpreter == null)
			{
				var ev = sequenceQueue.Dequeue();
				Sequence secuence = (ev.getParameter("Sequence") as Sequence);
				sequenceInterpreter = new SequenceInterpreter(secuence);
			}

			if(sequenceInterpreter != null){
	            sequenceInterpreter.Tick();
				if(sequenceInterpreter.SequenceFinished){
					Debug.Log("Sequence finished");
					this.sequenceInterpreter = null;
				}
			}
		}
	}
}
