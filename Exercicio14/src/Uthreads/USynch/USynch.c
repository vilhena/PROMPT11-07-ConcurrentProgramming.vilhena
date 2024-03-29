/////////////////////////////////////////////////////////////////
//
// CCISEL 
// 2007-2011
//
// UThread library:
//   User threads supporting cooperative multithreading.
//
// Authors:
//   Carlos Martins, Jo�o Trindade, Duarte Nunes, Jorge Martins
// 

#include "USynch.h"
#include "UThread.h"
#include "WaitBlock.h"

VOID EventInit (PEVENT Event, BOOL Signaled) {
	Event->Signaled = Signaled;
	InitializeListHead(&Event->Waiters);
}

VOID EventWait (PEVENT Event) {
	if (Event->Signaled == TRUE) {
		Event->Signaled = FALSE;
	} else {
		WAIT_BLOCK WaitBlock;
		WaitBlock.Thread = UtSelf();
		InsertTailList(&Event->Waiters, &WaitBlock.Link);
		UtDeactivate();
	}
}

VOID EventSignal (PEVENT Event) {
	if (IsListEmpty(&Event->Waiters)) {
		Event->Signaled = TRUE;
	} else {
		PWAIT_BLOCK WaitBlockPtr =
			CONTAINING_RECORD(RemoveHeadList(&Event->Waiters), WAIT_BLOCK, Link);
		UtActivate(WaitBlockPtr->Thread);
	}
}
