# Light_Puzzle

<img width="663" height="370" alt="image" src="https://github.com/user-attachments/assets/a78e3792-844a-4677-8ef1-0aa9453a4330" />

빛을 주제로 한 퍼즐 게임입니다. 

스테이지마다 존재하는 모든 Target을 맞추면 다음 스테이지로 넘어갈 수 있습니다. 

Unity 2D를 기반으로 제작하였습니다.

---

## 주요 기능

#### 1. 플레이어 이동 시스템

기본 이동, 점프, 사다리 오르기 지원

LightFlicker 시스템과 연동되어 조명 상태에 따라 이동 제한

사망 시 SpawnPoint로 즉시 Respawn

#### 2. 투사체 시스템 — 총알(Bullet)

Object Pool 기반 총알 자동 재활용

반사(Reflect), 굴절(Refract), 프리즘(Prism), 포탈(Portal) 등 다양한 상호작용

총알이 Target에 명중하면 Gate가 열릴 수 있는 퍼즐 구성

#### 3. 레이저(Laser) 퍼즐 시스템

Raycast 기반 빛의 경로 계산

반사, 굴절, 포탈 전이, 빛 분산(프리즘) 등 광학 기반 기믹 구현

LineRenderer + LightPool로 광선 시각화

Core 오브젝트를 빛으로 충전하는 퍼즐 요소 포함

#### 4. 프리즘(Prism) 분광 시스템

레이저 또는 총알이 프리즘을 통과하면
+15°, 0°, -15° 의 3갈래로 분리되어 새로운 경로 생성

#### 5. 포탈(Portal) 시스템

연결된 Portal 간 빛/총알이 매끄럽게 이동

포탈 출구에 닿은 레이저/총알은 방향 유지 상태로 재발사

#### 6. 조명(Light) 기반 환경 변화

LightFlicker 시스템: 씬별로 조명 깜빡임/꺼짐 연출

빛이 켜지면 플레이어 동작이 제한되는 공포·퍼즐 요소

LightBlock: 빛의 유무에 따라 바닥/벽이 ON/OFF 되는 플랫포머 기믹

#### 7. 빛 함정(Light Trap)

빛에 닿으면 플레이어 즉시 사망 (OverlapPoint로 감지)

RespawnZone에 닿으면 자동 respawn

#### 8. 카메라 시스템

인트로 스크롤 애니메이션 후 플레이어 추적

카메라 이동 범위 설정 및 부드러운 Lerp 이동

#### 10. 오브젝트 풀링(Object Pooling) 전면 사용

Bullet Pool

Laser Pool

Portal Laser Pool

Light Prefab Pool

----

## 기술스택

#### Game Engine

Unity 2D (2022.3.62f1)

LineRenderer + URP 2D Light2D

#### Programming

C#

Unity Events

Coroutine 기반 비동기 처리

Object Pooling 최적화 패턴

Raycast 기반 광학 시뮬레이션

#### Graphics / Visual

Light2D (URP 2D lighting)

Tilemap 기반 레벨 구성

LineRenderer로 광선 표현

#### Gameplay Architecture

Singleton GameManager / LineSpawner / LightFlicker

Player FSM 구조 (Idle / Move / Jump / Climb / Shoot / Laser / Disabled)

Event-Based Scene Loading

#### Puzzle / Interaction Logic

Reflect / Refract / Prism / Portal 상호작용

Raycast 기반 다중 Bounce(반사) 처리

#### 형상 관리

GitHub

