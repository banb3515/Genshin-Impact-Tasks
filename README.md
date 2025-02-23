# **원신 태스크(Genshin Impact Tasks)**

## **목차**

1. [Genshin Impact Tasks란?](#Genshin-Impact-Tasks란)
2. [시연 영상](#시연-영상)
3. [개발 정보](#개발-정보)
4. [지원 플랫폼](#지원-플랫폼)
5. [버전별 업데이트 사항](#버전별-업데이트-사항)

### **Genshin Impact Tasks란?**

-   Genshin Impact Tasks는 게임 [원신](https://genshin.mihoyo.com/ko)의 도우미 앱 입니다.
-   할 일을 기록해두거나 게임 플레이에 도움이 되는 기능을 사용할 수 있습니다.
-   할 일 목록은 Firebase - Realtime Database에 저장되며, Android와 Windows와 연동이 가능합니다.

### **시연 영상**

-   [Android 시연 영상](./Screenshots/Android.mp4)

-   [Windows 시연 영상](./Screenshots/Windows.mp4)

### **개발 정보**

-   언어 - [C#](https://docs.microsoft.com/ko-kr/dotnet/csharp/)
-   앱 프레임워크 - [자마린(Xamarin)](https://docs.microsoft.com/ko-kr/xamarin/get-started/what-is-xamarin)
-   사용된 패키지
    -   [Rg.Plugins.Popup](https://github.com/rotorgames/Rg.Plugins.Popup)
    -   [sqlite-net-pcl](https://github.com/praeclarum/sqlite-net)
    -   [Syncfusion.Xamarin.SfTreeView](https://help.syncfusion.com/xamarin/treeview/overview)
    -   [Syncfusion.Xamarin.SfTabView](https://help.syncfusion.com/xamarin/tabbed-view/overview)
    -   [Xamarin.Essentials](https://github.com/xamarin/Essentials)
    -   [FirebaseDatabase.net](https://github.com/step-up-labs/firebase-database-dotnet)
    -   [Xam.Plugin.LatestVersion](https://github.com/step-up-labs/firebase-database-dotnet)
    -   [Plugin.LocalNotification](https://github.com/step-up-labs/firebase-database-dotnet)

---

### **지원 플랫폼**

-   Android
    -   권장 버전: Android 10.0 (Q)
    -   최소 버전: Android 7.1 (Nougat)
-   Windows
    -   권장 버전: Windows 10, version 1903 (10.0, 빌드 18362)
    -   최소 버전: Windows 10, version 1809 (10.0, 빌드 17763)
-   iOS 미지원

### **버전별 업데이트 사항**

<details>
 <summary><b>v1.0</b></summary>
 <hr>
 <blockquote>1. 앱 출시</blockquote>
 <hr>
</details>
<details>
 <summary><b>v1.1</b></summary>
 <hr>
 <blockquote>1. 동기화 진행 중 다른 기능 비활성화</blockquote>
 <hr>
</details>
<details>
 <summary><b>v1.2</b></summary>
 <hr>
 <blockquote>1. 아이콘 추가</blockquote>
 <blockquote> &nbsp;&nbsp;&nbsp;- 캐릭터, NPC, 보스, 몬스터, 성유물, 광물, 채집물, 음식, 돌파 재료, 특성 레벨업 재료, 재료</blockquote>
 <blockquote>2. 아이콘 삭제</blockquote>
 <blockquote> &nbsp;&nbsp;&nbsp;- 1, 2성 무기 레벨업 재료, 2, 3성 캐릭터 레벨업 재료</blockquote>
 <hr>
</details>
<details>
 <summary><b>v1.3</b></summary>
 <hr>
 <blockquote>1. 파밍 탭 추가</blockquote>
 <blockquote> &nbsp;&nbsp;&nbsp;- 광물, 채집물의 리젠 완료 시간을 알려주며 리젠 완료 시간이 되었을 때 알림이 울립니다.</blockquote>
 <blockquote>2. 타이머 탭 제거</blockquote>
 <blockquote>3. 로딩 화면 추가</blockquote>
 <blockquote>4. UWP 자동 시작 서비스 추가</blockquote>
 <blockquote>5. 패키지 업데이트</blockquote>
 <blockquote> &nbsp;&nbsp;&nbsp;- FirebaseDatabase.net, Xamarin.Forms, Xamarin.Essentials</blockquote>
 <hr>
  <details>
   <summary><b>v1.3.1</b></summary>
   <hr>
   <blockquote>1. 인터넷에 연결되어 있을 때 주간 할 일 초기화가 안되는 점 수정</blockquote>
   <blockquote>2. (UWP) 앱 시작 시 무한로딩 현상 수정</blockquote>
  </details>
 <hr>
</details>
<details>
 <summary><b>v1.4</b></summary>
 <hr>
 <blockquote>1. 아이콘 추가 - NPC 클로리스</blockquote>
 <blockquote>2. 앱 시작 시 업데이트 확인 추가</blockquote>
 <blockquote>3. 자동 동기화, 수동 동기화가 겹칠 때 발생하는 버그 수정</blockquote>
 <blockquote>4. 앱 시작 시 업데이트 확인 추가</blockquote>
 <blockquote>5. 안드로이드 변경 사항</blockquote>
 <blockquote> &nbsp;&nbsp;&nbsp;a. 요일별로 상태바 색상 변경</blockquote>
 <blockquote> &nbsp;&nbsp;&nbsp;b. 알림 버그 수정</blockquote>
 <hr>
</details>
<details>
 <summary><b>v1.5</b></summary>
 <hr>
 <blockquote>1. 이미 초기화된 할 일이 다른 기기에서 다시 한번 초기화되는 버그 수정</blockquote>
 <blockquote>2. 탭 목록 애니메이션 변경</blockquote>
 <blockquote>3. 안드로이드 변경 사항</blockquote>
 <blockquote> &nbsp;&nbsp;&nbsp;a. 할 일 위젯 추가 (Beta)</blockquote>
 <hr>
</details>
<details>
 <summary><b>v1.6</b></summary>
 <hr>
 <blockquote>1. 안드로이드 변경 사항</blockquote>
 <blockquote> &nbsp;&nbsp;&nbsp;a. 할 일 위젯 업데이트</blockquote>
 <hr>
</details>
<details>
 <summary><b>v1.7</b></summary>
 <hr>
 <blockquote>1. 추가 버튼이 비활성화 되는 버그 수정</blockquote>
 <blockquote>2. 안드로이드 변경 사항</blockquote>
 <blockquote> &nbsp;&nbsp;&nbsp;a. 동기화 미설정 상태에서 위젯 할 일 탭 시 보이는 로딩 화면 제거</blockquote>
 <hr>
</details>
