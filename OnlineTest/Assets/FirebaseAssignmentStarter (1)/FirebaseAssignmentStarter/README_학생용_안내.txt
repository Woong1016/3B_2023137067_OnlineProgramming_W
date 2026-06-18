Firebase 데이터 연동 과제 기본 코드 안내
======================================

1. 포함된 스크립트
-----------------
UserData.cs
- 회원가입 시 저장할 기본 유저 데이터 클래스

UserRegister.cs
- 닉네임 회원가입
- 중복 닉네임 검사
- Firebase에 UserInfo 저장
- PlayerPrefs에 UserKey, UserNickName 저장

UserLogin.cs
- 닉네임 로그인
- Firebase에서 닉네임 검색
- PlayerPrefs에 UserKey, UserNickName 저장

ShopManager.cs
- 로그인 유저의 Coin, Inventory 불러오기
- Potion, Bomb, Ticket 구매
- 코인 차감 및 Inventory 증가

InventoryManager.cs
- Inventory 불러오기
- Potion, Bomb, Ticket 개수 표시
- 아이템 사용 시 개수 감소


2. 사전 준비
------------
Unity 프로젝트에 아래 패키지/플러그인이 필요합니다.

- Firebase Realtime Database SDK
- Newtonsoft.Json
- UnityMainThreadDispatcher


3. Firebase 주소 수정
--------------------
각 스크립트 Inspector에 있는 databaseUrl 값을 본인 Firebase Realtime Database 주소로 바꾸세요.

예시:
https://본인프로젝트-default-rtdb.asia-southeast1.firebasedatabase.app/


4. UI 연결
----------
회원가입/로그인 화면:
- InputField 1개
- Text 1개
- 회원가입 Button
- 로그인 Button

상점 화면:
- CoinText
- MessageText
- PotionBuyButton
- BombBuyButton
- TicketBuyButton

인벤토리 화면:
- PotionCountText
- BombCountText
- TicketCountText
- MessageText
- UsePotionButton
- UseBombButton
- UseTicketButton


5. 버튼 연결
------------
RegisterButton -> UserRegister.OnClickRegister
LoginButton -> UserLogin.OnClickLogin
PotionBuyButton -> ShopManager.OnClickBuyPotion
BombBuyButton -> ShopManager.OnClickBuyBomb
TicketBuyButton -> ShopManager.OnClickBuyTicket
UsePotionButton -> InventoryManager.OnClickUsePotion
UseBombButton -> InventoryManager.OnClickUseBomb
UseTicketButton -> InventoryManager.OnClickUseTicket


6. 테스트 순서
--------------
1) 닉네임 입력 후 회원가입
2) Firebase 콘솔에서 UserInfo 생성 확인
3) 같은 닉네임으로 로그인
4) 상점에서 Potion 구매
5) Firebase에서 Coin 감소, Inventory 증가 확인
6) 인벤토리 화면에서 Potion 개수 확인
7) Potion 사용
8) Firebase에서 Potion 개수 감소 확인


7. 주의 사항
------------
이 코드는 수업용 기본 코드입니다.
실제 서비스에서는 클라이언트에서 코인과 아이템을 직접 수정하면 보안 문제가 생길 수 있습니다.
실제 출시용 게임에서는 서버 검증 또는 Firebase Cloud Functions를 사용하는 것이 좋습니다.
