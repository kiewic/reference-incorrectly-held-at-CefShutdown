# FATAL:shutdown_checker.cc: Check failed: !IsCefShutdown(). Object reference incorrectly held at CefShutdown

Exception/assert-failure using CefhSharp OffScreen:

```
[0204/140114.330:FATAL:shutdown_checker.cc(52)] Check failed: !IsCefShutdown(). Object reference incorrectly held at CefShutdown
[0204/140726.019:ERROR:browser_process_sub_thread.cc(221)] Waited 42 ms for network service
```

## Version 79.1.350

Hello, I was able to reproduce the issue with CEF 79.1.350. I have opened created issue XYZ.

To reproduce this issue, replace with Debug binaries of CEF. For example [Debug cef_binary_79.1.35+gfebbb4a+chromium-79.0.3945.130_windows64](cef_binary_79.1.35+gfebbb4a+chromium-79.0.3945.130_windows64).

Version 79.1.350 crash callsatck:

```
00 libcef!logging::LogMessage::~LogMessage+0x647 [Y:\work\CEF3_git\chromium\src\base\logging.cc @ 954]
01 libcef!shutdown_checker::AssertNotShutdown+0x78 [Y:\work\CEF3_git\chromium\src\cef\libcef_dll\shutdown_checker.cc @ 54]
02 libcef!CefLifeSpanHandlerCToCpp::OnBeforeClose+0x29 [Y:\work\CEF3_git\chromium\src\cef\libcef_dll\ctocpp\life_span_handler_ctocpp.cc @ 150]
03 libcef!CefBrowserHostImpl::DestroyBrowser+0xf3 [Y:\work\CEF3_git\chromium\src\cef\libcef\browser\browser_host_impl.cc @ 1539]
04 libcef!CefBrowserInfoManager::DestroyAllBrowsers+0x13f [Y:\work\CEF3_git\chromium\src\cef\libcef\browser\browser_info_manager.cc @ 332]
05 libcef!CefContext::FinishShutdownOnUIThread+0x7a [Y:\work\CEF3_git\chromium\src\cef\libcef\browser\context.cc @ 606]
06 libcef!base::OnceCallback<void ()>::Run+0x61 [Y:\work\CEF3_git\chromium\src\base\callback.h @ 98]
07 libcef!base::TaskAnnotator::RunTask+0x185 [Y:\work\CEF3_git\chromium\src\base\task\common\task_annotator.cc @ 142]
08 libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::DoWorkImpl+0x1b5 [Y:\work\CEF3_git\chromium\src\base\task\sequence_manager\thread_controller_with_message_pump_impl.cc @ 366]
09 libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::DoSomeWork+0x61 [Y:\work\CEF3_git\chromium\src\base\task\sequence_manager\thread_controller_with_message_pump_impl.cc @ 221]
0a libcef!base::MessagePumpForUI::DoRunLoop+0x143 [Y:\work\CEF3_git\chromium\src\base\message_loop\message_pump_win.cc @ 219]
0b libcef!base::MessagePumpWin::Run+0xa4 [Y:\work\CEF3_git\chromium\src\base\message_loop\message_pump_win.cc @ 77]
0c libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::Run+0x129 [Y:\work\CEF3_git\chromium\src\base\task\sequence_manager\thread_controller_with_message_pump_impl.cc @ 467]
0d libcef!base::RunLoop::Run+0x30e [Y:\work\CEF3_git\chromium\src\base\run_loop.cc @ 158]
0e libcef!CefUIThread::ThreadMain+0x77 [Y:\work\CEF3_git\chromium\src\cef\libcef\common\main_delegate.cc @ 395]
0f libcef!base::`anonymous namespace'::ThreadFunc+0xcc [Y:\work\CEF3_git\chromium\src\base\threading\platform_thread_win.cc @ 105]
10 KERNEL32!BaseThreadInitThunk+0x14 [base\win32\client\thread.c @ 64]
11 ntdll!RtlUserThreadStart+0x21 [minkernel\ntdll\rtlstrt.c @ 1153]
```

Thread 0 is already doing CefShutdown:

```
00 ntdll!ZwWaitForSingleObject+0x14 [minkernel\ntdll\daytona\objfre\amd64\usrstubs.asm @ 211]
01 KERNELBASE!WaitForSingleObjectEx+0x93 [minkernel\kernelbase\synch.c @ 1328]
02 libcef!base::WaitableEvent::Wait+0xab [Y:\work\CEF3_git\chromium\src\base\synchronization\waitable_event_win.cc @ 69]
03 libcef!CefContext::Shutdown+0x10f [Y:\work\CEF3_git\chromium\src\cef\libcef\browser\context.cc @ 481]
04 libcef!CefShutdown+0xb5 [Y:\work\CEF3_git\chromium\src\cef\libcef\browser\context.cc @ 272]
05 CefSharp_Core!DomainBoundILStubClass.IL_STUB_PInvoke()+0x68
06 CefSharp_Core!CefSharp::Cef::Shutdown+0x283 [c:\projects\cefsharp\cefsharp.core\cef.h @ 89]
07 CefSharpOffscreenRepro!CefSharpOffscreenRepro.Program.Main(System.String[])+0x28b*** WARNING: Unable to verify checksum for D:\repos\reference-incorrectly-held-at-CefShutdown\CefSharpOffscreenRepro\bin\x64\Debug\CefSharpOffscreenRepro.exe
 [D:\repos\reference-incorrectly-held-at-CefShutdown\CefSharpOffscreenRepro\Program.cs @ 64]
08 CefSharpOffscreenRepro!CefSharpOffscreenRepro.Program.Main(System.String[])+0x1ae
09 clr!CallDescrWorkerInternal+0x83 [f:\dd\ndp\clr\src\vm\amd64\CallDescrWorkerAMD64.asm @ 97]
0a clr!CallDescrWorkerWithHandler+0x4e [f:\dd\ndp\clr\src\vm\callhelpers.cpp @ 89]
0b clr!MethodDescCallSite::CallTargetWorker+0x102 [f:\dd\ndp\clr\src\vm\callhelpers.cpp @ 655]
0c clr!MethodDescCallSite::Call_RetArgSlot+0x5 [f:\dd\ndp\clr\src\vm\callhelpers.h @ 423]
0d clr!RunMain+0x266 [f:\dd\ndp\clr\src\vm\assembly.cpp @ 2659]
0e clr!Assembly::ExecuteMainMethod+0xb7 [f:\dd\ndp\clr\src\vm\assembly.cpp @ 2780]
0f clr!SystemDomain::ExecuteMainMethod+0x643 [f:\dd\ndp\clr\src\vm\appdomain.cpp @ 3755]
10 clr!ExecuteEXE+0x3f [f:\dd\ndp\clr\src\vm\ceemain.cpp @ 3053]
11 clr!_CorExeMainInternal+0xb2 [f:\dd\ndp\clr\src\vm\ceemain.cpp @ 2887]
12 clr!_CorExeMain+0x14 [f:\dd\ndp\clr\src\vm\ceemain.cpp @ 2814]
13 mscoreei!_CorExeMain+0x112 [f:\dd\ndp\clr\src\dlls\shim\shim.cpp @ 6425]
14 MSCOREE!_CorExeMain_Exported+0x6c [onecore\com\netfx\windowsbuilt\shell_shim\v2api.cpp @ 1223]
15 KERNEL32!BaseThreadInitThunk+0x14 [base\win32\client\thread.c @ 64]
16 ntdll!RtlUserThreadStart+0x21 [minkernel\ntdll\rtlstrt.c @ 1153]
```


## Version 75.1.14

I am loading a page, and as soon as it completes loading, I call `Cef.Shutdown()` and the process exits. You can see the source code [here](CefSharpOffscreenRepro/Program.cs).

The issue reproduces in CefSharp 75, but I haven't been able to test with CefSharp 79. I see [similar CEF fixes](https://bitbucket.org/chromiumembedded/cef/commits/c7345f21bede2ad5a6831fcc1457494e589c7c4d?at=3904), but in those cases people mention that the **process crashes**, and in my case, the warning is just printed to the output stream or it only breaks when the app is attached to a debugger.

The faulting callstack is:

```
(e9ec.f490): Illegal instruction - code c000001d (first chance)
[0206/132058.262:ERROR:dns_config_service_win.cc(758)] DNS config watch failed.
(e9ec.f490): Illegal instruction - code c000001d (!!! second chance !!!)
#  7  Id: e9ec.f490 Suspend: 1 Teb: 000000d9`79bc7000 Unfrozen "CrBrowserMain"
 # Call Site
00 libcef!logging::LogMessage::~LogMessage
01 libcef!cef_log
02 CefSharp_Core!cef::logging::LogMessage::~LogMessage
03 CefSharp_Core!shutdown_checker::AssertNotShutdown
04 CefSharp_Core!CefPopupFeaturesTraits::init
05 libcef!CefJSDialogHandlerCToCpp::OnDialogClosed
06 libcef!CefBrowserHostImpl::DestroyBrowser
07 libcef!CefBrowserInfoManager::DestroyAllBrowsers
08 libcef!CefContext::FinishShutdownOnUIThread
09 libcef!base::TaskAnnotator::RunTask
0a libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::DoWorkImpl
0b libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::DoSomeWork
0c libcef!base::MessagePumpForUI::DoRunLoop
0d libcef!base::MessagePumpWin::Run
0e libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::Run
0f libcef!base::RunLoop::RunWithTimeout
10 libcef!CefUIThread::ThreadMain
11 libcef!base::`anonymous namespace'::ThreadFunc
12 KERNEL32!BaseThreadInitThunk
13 ntdll!RtlUserThreadStart
```

But notice that at the same time the main thread has already started the shutdown:

```
   0  Id: e9ec.98dc Suspend: 1 Teb: 000000d9`79bb9000 Unfrozen
 # Call Site
00 ntdll!ZwWaitForSingleObject
01 KERNELBASE!WaitForSingleObjectEx
02 libcef!base::WaitableEvent::Wait
03 libcef!CefContext::Shutdown
04 libcef!CefShutdown
05 CefSharp_Core!CefShutdown
06 CefSharp_Core!DomainBoundILStubClass.IL_STUB_PInvoke()
07 CefSharp_Core!CefSharp::Cef::Shutdown
08 Microsoft_Mashup_CefHelper!Microsoft.Mashup.CefHelper.Program.Main(System.String[])
09 Microsoft_Mashup_CefHelper!Microsoft.Mashup.CefHelper.Program.Main(System.String[])
0a clr!CallDescrWorkerInternal
0b clr!CallDescrWorkerWithHandler
0c clr!MethodDescCallSite::CallTargetWorker
0d clr!MethodDescCallSite::Call_RetArgSlot
0e clr!RunMain
0f clr!Assembly::ExecuteMainMethod
10 clr!SystemDomain::ExecuteMainMethod
11 clr!ExecuteEXE
12 clr!_CorExeMainInternal
13 clr!_CorExeMain
14 mscoreei!_CorExeMain
15 MSCOREE!_CorExeMain_Exported
16 KERNEL32!BaseThreadInitThunk
17 ntdll!RtlUserThreadStart
```

The faulting callstack is not always the same. You can see some variations in [here](https://github.com/kiewic/reference-incorrectly-held-at-CefShutdown#failure-callstack-variations).

# Failure callstack variations

Sometimes the failure happens at:

```
00 libcef!logging::LogMessage::~LogMessage
*** WARNING: Unable to verify checksum for d:\repos\power-query-3\bin\x64\debug\Product\Binaries\Client\bin\CefSharp.Core.dll
01 libcef!cef_log
02 CefSharp_Core!cef::logging::LogMessage::~LogMessage
03 CefSharp_Core!shutdown_checker::AssertNotShutdown
04 CefSharp_Core!CefBrowserCToCpp::IsPopup
05 CefSharp_Core!DomainBoundILStubClass.IL_STUB_PInvoke(IntPtr)
06 CefSharp_Core!CefSharp::Internals::ClientAdapter::OnBeforeClose
07 CefSharp_Core!DomainBoundILStubClass.IL_STUB_ReversePInvoke(Int64, IntPtr)
08 clr!UMThunkStub
09 CefSharp_Core!CefPopupFeaturesTraits::init
0a libcef!CefJSDialogHandlerCToCpp::OnDialogClosed
0b libcef!CefBrowserHostImpl::DestroyBrowser
0c libcef!CefBrowserHostImpl::CloseContents
0d libcef!IPC::MessageT<ViewHostMsg_ClosePage_ACK_Meta,std::__1::tuple<>,void>::Dispatch<content::RenderViewHostImpl,content::RenderViewHostImpl,void,void (content::RenderViewHostImpl::*)()>
0e libcef!content::RenderViewHostImpl::OnMessageReceived
0f libcef!content::RenderProcessHostImpl::OnMessageReceived
10 libcef!IPC::ChannelProxy::Context::OnDispatchMessage
11 libcef!base::TaskAnnotator::RunTask
12 libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::DoWorkImpl
13 libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::DoSomeWork
14 libcef!base::MessagePumpForUI::DoRunLoop
15 libcef!base::MessagePumpWin::Run
16 libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::Run
17 libcef!base::RunLoop::RunWithTimeout
18 libcef!CefUIThread::ThreadMain
19 libcef!base::`anonymous namespace'::ThreadFunc
1a KERNEL32!BaseThreadInitThunk
1b ntdll!RtlUserThreadStart
```

And sometimes it happens at:

```
00 libcef!logging::LogMessage::~LogMessage
*** WARNING: Unable to verify checksum for d:\repos\power-query-3\bin\x64\debug\Product\Binaries\Client\bin\CefSharp.Core.dll
01 libcef!cef_log
02 CefSharp_Core!cef::logging::LogMessage::~LogMessage
03 CefSharp_Core!shutdown_checker::AssertNotShutdown
04 CefSharp_Core!CefBrowserCToCpp::~CefBrowserCToCpp
05 CefSharp_Core!CefBrowserCToCpp::`vbase destructor'
06 CefSharp_Core!CefCToCppRefCounted<CefBrowserCToCpp,CefBrowser,_cef_browser_t>::WrapperStruct::~WrapperStruct
07 CefSharp_Core!CefCToCppRefCounted<CefBrowserCToCpp,CefBrowser,_cef_browser_t>::WrapperStruct::`scalar deleting destructor'
08 CefSharp_Core!CefCToCppRefCounted<CefBrowserCToCpp,CefBrowser,_cef_browser_t>::Release
09 CefSharp_Core!DomainBoundILStubClass.IL_STUB_PInvoke(IntPtr)
0a CefSharp_Core!scoped_refptr<CefBrowser>::~scoped_refptr<CefBrowser>
0b CefSharp_Core!CefSharp::Internals::ClientAdapter::GetResourceRequestHandler
0c CefSharp_Core!DomainBoundILStubClass.IL_STUB_ReversePInvoke(Int64, Int64, IntPtr, IntPtr, IntPtr, SByte, SByte, Int64, Int64)
0d clr!UMThunkStub
0e CefSharp_Core!std::vector<scoped_refptr<CefX509Certificate>,std::allocator<scoped_refptr<CefX509Certificate> > >::push_back
0f libcef!CefRequestHandlerCToCpp::GetResourceRequestHandler
10 libcef!net_service::`anonymous namespace'::InterceptedRequestHandlerWrapper::OnBeforeRequest
11 libcef!net_service::InterceptedRequest::Restart
12 libcef!net_service::ProxyURLLoaderFactory::CreateLoaderAndStart
13 libcef!network::mojom::URLLoaderFactoryStubDispatch::Accept
14 libcef!mojo::internal::MultiplexRouter::ProcessIncomingMessage
15 libcef!mojo::internal::MultiplexRouter::Accept
16 libcef!mojo::Connector::DispatchMessageW
17 libcef!mojo::Connector::ReadAllAvailableMessages
18 libcef!mojo::SimpleWatcher::OnHandleReady
19 libcef!mojo::SimpleWatcher::Context::Notify
1a libcef!mojo::SimpleWatcher::Context::CallNotify
1b libcef!mojo::core::WatcherDispatcher::InvokeWatchCallback
1c libcef!mojo::core::Watch::InvokeCallback
1d libcef!mojo::core::RequestContext::~RequestContext
1e libcef!mojo::core::NodeChannel::OnChannelMessage
1f libcef!mojo::core::Channel::TryDispatchMessage
20 libcef!mojo::core::Channel::OnReadComplete
21 libcef!mojo::core::`anonymous namespace'::ChannelWin::OnIOCompleted
22 libcef!base::MessagePumpForIO::WaitForIOCompletion
23 libcef!base::MessagePumpForIO::DoRunLoop
24 libcef!base::MessagePumpWin::Run
25 libcef!base::sequence_manager::internal::ThreadControllerWithMessagePumpImpl::Run
26 libcef!base::RunLoop::RunWithTimeout
27 libcef!content::BrowserProcessSubThread::IOThreadRun
28 libcef!base::Thread::ThreadMain
29 libcef!base::`anonymous namespace'::ThreadFunc
2a KERNEL32!BaseThreadInitThunk
2b ntdll!RtlUserThreadStart
```
