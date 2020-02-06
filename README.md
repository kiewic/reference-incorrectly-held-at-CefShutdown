# FATAL:shutdown_checker.cc: Check failed: !IsCefShutdown(). Object reference incorrectly held at CefShutdown

Assert fail:

```
[0204/140114.330:FATAL:shutdown_checker.cc(52)] Check failed: !IsCefShutdown(). Object reference incorrectly held at CefShutdown
[0204/140726.019:ERROR:browser_process_sub_thread.cc(221)] Waited 42 ms for network service
```

We are loading a page, and as soon as it completes loading, we terminate the process. You can see the repro source code [here](https://todo).

The issue reproduces in CefSharp 75, but I haven't been able to reproduce with CefSharp 79. I see some CEF fixes with similar callstacks, but in those cases they are talking about **process crashes**, and in my case, the warning is just printed to console or it only breaks when the app is attached to a debugger.

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

Notice that the Main thread already started the shutdown:

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


