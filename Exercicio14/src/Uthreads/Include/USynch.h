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

#pragma once

#include <Windows.h>

#ifndef USYNCH_DLL
#define USYNCH_API __declspec(dllimport)
#else
#define USYNCH_API __declspec(dllexport)
#endif

typedef struct Event {
	BOOL Signaled;
	LIST_ENTRY Waiters;
} EVENT, *PEVENT;

#ifdef __cplusplus
extern "C" {
#endif

USYNCH_API
VOID EventInit (PEVENT Event, BOOL Signaled);

FORCEINLINE
BOOL EventValue (PEVENT Event) {
	return Event->Signaled; 
}

USYNCH_API
VOID EventWait (PEVENT Event);

USYNCH_API
VOID EventSignal (PEVENT Event);

#ifdef __cplusplus
} // extern "C"
#endif
