# yQuant

> 시장 데이터, 투자 신호, 전략 연구, 포트폴리오 정책, 브로커 실행, 운영 관찰을 연결하는 개인 투자 연구·운영 시스템

yQuant는 예측의 과장이 아닌 투자 의사결정 과정의 구조화와 검증 가능성을 목표로 함

시장 데이터 수집, 금융 텍스트 해석, 전략 아이디어 실험, 비용과 낙폭을 포함한 백테스트, 계좌별 주문 조합, 브로커 실행, 운영 상태 관찰을 하나의 흐름으로 연결함

## Core principles

- **Evidence first** — 좋은 아이디어보다 재현 가능한 근거를 우선함
- **Separation of concerns** — 데이터, 신호, 전략, 주문 정책, 브로커 실행, 영속화의 책임을 분리함
- **Human control** — 자동화를 판단 보조와 운영 일관성 확보에 활용함
- **Broker-neutral contracts** — 브로커별 차이를 실행 계층에 격리하고 공통 메시지 계약을 유지함
- **Observable operations** — 주문, 보유, 잔고, 시세, 서비스 상태의 추적 가능성을 확보함
- **Research feedback loop** — 실패 원인을 다음 연구 사이클의 입력으로 반영함

## System flow

```text
시장 데이터 · 종목 정보 · 실시간 호가
                ↓
금융 텍스트 기반 시장 신호
                ↓
전략 정의 · 구현 · 검증 · 백테스트
                ↓
포트폴리오 비중 결정 · 주문 조합
                ↓
브로커 · 거래소 실행
                ↓
운영 콘솔 · 상태 관찰 · 영속 기록
```

## Start here

| 목적 | 시작 저장소 | 역할 |
| --- | --- | --- |
| 전체 시스템 상태 확인 | [yquant-console](https://github.com/yoonbae81/yquant-console) | 계좌, 보유종목, 시세, 주문, 예약 주문, 서비스 상태를 다루는 운영 콘솔 |
| 시장 데이터 운영 | [yquant-data](https://github.com/yoonbae81/yquant-data) | OHLCV, 종목 메타데이터, 차트 데이터의 수집·발행·배포 |
| 전략 연구 | [yquant-forge](https://github.com/yoonbae81/yquant-forge) | 전략 아이디어와 연구 파이프라인의 작업 공간 |
| 신호를 주문으로 연결 | [yquant-portfolio](https://github.com/yoonbae81/yquant-portfolio) | 전략 신호의 계좌별 주문 조합과 주문 요청 발행 |
| 브로커 연동 구조 확인 | [yquant-gateway](https://github.com/yoonbae81/yquant-gateway) | 브로커 공통 계약, 중앙 projection, 주문 관련 작업의 기반 |
| 인프라와 프로토콜 확인 | [yquant-infra](https://github.com/yoonbae81/yquant-infra) | PostgreSQL, Valkey, 운영 스크립트, 서비스 간 프로토콜 거버넌스 |

## Architecture

yQuant는 빠르게 변하는 운영 상태와 오래 보존할 기록을 분리하는 구조에 기반함

```text
                     ┌──────────────────────────┐
                     │       yquant-data        │
                     │ OHLCV · ticker · charts  │
                     └────────────┬─────────────┘
                                  │
                                  ▼
┌──────────────┐       ┌──────────────────────────┐       ┌──────────────────┐
│ yquant-pulse │ ───▶  │    yquant-portfolio      │ ───▶  │  Broker gateways │
│ text signals │       │ policy · sizing · orders │       │ KIS · UPB · Toss │
└──────────────┘       └────────────┬─────────────┘       └────────┬─────────┘
                                    │                                │
                                    ▼                                ▼
                         ┌────────────────────────────────────────────┐
                         │                   Valkey                   │
                         │ live state · streams · commands · events   │
                         └───────────────────┬────────────────────────┘
                                             │
                                             ▼
                         ┌────────────────────────────────────────────┐
                         │              yquant-gateway                │
                         │ projection · scheduling · reconciliation   │
                         └───────────────────┬────────────────────────┘
                                             │
                                             ▼
                         ┌────────────────────────────────────────────┐
                         │                PostgreSQL                  │
                         │ durable orders · fills · deposits · snapshots│
                         └───────────────────┬────────────────────────┘
                                             │
                                             ▼
                                  ┌────────────────────┐
                                  │   yquant-console   │
                                  │ observe · request  │
                                  └────────────────────┘
```

### Valkey

Valkey는 최신 운영 상태와 서비스 간 실시간 통신을 담당함

| 영역 | Valkey 역할 |
| --- | --- |
| 최신 계좌 상태 | 계좌, 보유종목, 잔고의 최신 값과 인덱스 |
| 주문 흐름 | 주문 command, 주문 event, 체결 event stream |
| 시장 데이터 | 현재가, 환율, 종목 메타데이터, 운영 universe |
| 예약 실행 | 예약 주문 intent, due schedule, 지연 주문 상태 |
| 서비스 통신 | 브로커 Gateway, Portfolio, Console, 중앙 Gateway 간 메시지 계약 |

Valkey의 데이터는 현재 시점의 상태와 즉시 반응해야 하는 이벤트에 기반함

브로커별 Gateway는 PostgreSQL에 직접 연결하지 않고 Valkey 계약을 통해 계좌·주문·시세 상태를 게시함

### PostgreSQL

PostgreSQL은 장기 보존과 이력 조회가 필요한 기록을 담당함

| 영역 | PostgreSQL 역할 |
| --- | --- |
| 주문 이력 | 주문 요청 이후의 상태 변화와 이벤트 기록 |
| 체결 이력 | 체결 결과와 평균 체결가, 실현 손익 기록 |
| 입출금 기록 | 계좌 현금 흐름의 영속 기록 |
| 스냅샷 | 일별 자산 상태와 성과 조회를 위한 기록 |
| 운영 조회 | Console과 분석 계층의 이력 기반 조회 |

PostgreSQL은 최신 보유종목이나 최신 잔고의 운영 기준 저장소가 아닌 내구성 있는 기록 저장소로 동작함

### Write ownership

데이터 정합성은 쓰기 책임의 분리에 기반함

| Component | Write responsibility |
| --- | --- |
| Console | Valkey stream을 통한 주문·예약 주문 요청 |
| Portfolio | Valkey stream을 통한 계좌별 주문 요청 |
| Broker gateways | Valkey의 최신 계좌 상태와 주문·체결 이벤트 |
| yquant-gateway | Valkey event의 PostgreSQL projection과 snapshot 기록 |
| yquant-data | OHLCV, ticker metadata, chart 관련 데이터 생산 |

Console은 PostgreSQL을 조회에 사용하며 직접 쓰기를 수행하지 않음

브로커 Gateway는 실행 결과를 Valkey에 게시하며 PostgreSQL 기록은 중앙 yquant-gateway가 담당함

## Repository map

### Market data and market structure

| Repository | Focus |
| --- | --- |
| [yquant-data](https://github.com/yoonbae81/yquant-data) | 시장별 OHLCV 수집, 월별 저장, 범위 단위 Parquet 발행, 차트 데이터 생성 |
| [yquant-ticker](https://github.com/yoonbae81/yquant-ticker) | 한국 시장 세금·종목 마스터 데이터 생성 |

데이터 계층은 단순 가격 조회를 넘어 전략과 신호 검증을 위한 연구에 기반함

시간대별 가격, 종목 분류, 차트용 데이터의 일관된 공급을 담당함

### Research and evaluation

| Repository | Focus |
| --- | --- |
| [yquant-forge](https://github.com/yoonbae81/yquant-forge) | 전략 아이디어의 계획, 정의, 구현, 검증, 백테스트, 분석, 피드백을 위한 연구 작업 공간 |
| [yquant-proof](https://github.com/yoonbae81/yquant-proof) | 재현 가능한 시장 평가기와 런타임 정체성 검증을 위한 실험적 기반 |
| [yquant-orderbook](https://github.com/yoonbae81/yquant-orderbook) | 실시간 호가·체결 데이터 수집과 체결 모델 기반의 전략 연구 |

연구 계층은 전략 개수 확대보다 전략 버전별 근거 축적에 기반함

수익률뿐 아니라 최대 낙폭, 거래 빈도, 비용, 시장 국면, 구현 계약 준수, 반복 실패 원인을 기록함

### Market intelligence

| Repository | Focus |
| --- | --- |
| [yquant-pulse](https://github.com/yoonbae81/yquant-pulse) | 금융 텍스트의 구조화, 자산군·카테고리·ETF 영향 매핑, 규칙 기반 점수화, 비중 조정 후보 산출 |

Pulse는 단순 요약 도구가 아닌 시장 텍스트의 의사결정용 구조 데이터화에 기반함

텍스트 수집, 정제, 의미 단위 분할, 원인·결과 탐지, 행동 가능성 판정, universe 매핑, 점수화, 비중 조정 후보 산출을 순차적으로 수행함

런타임 단계는 결정론적 규칙과 추적 가능한 근거에 기반함

### Portfolio and execution

| Repository | Focus |
| --- | --- |
| [yquant-portfolio](https://github.com/yoonbae81/yquant-portfolio) | 외부 전략 신호의 수신, 계좌별 주문 조합, 노출·리스크 기반 수량 산정, 주문 요청 발행 |
| [yquant-gateway](https://github.com/yoonbae81/yquant-gateway) | 공통 Valkey 메시지 계약, 계좌·잔고·보유 최신 상태, 중앙 projection, 예약·지연 주문 관리 |
| [yquant-gateway-kis](https://github.com/yoonbae81/yquant-gateway-kis) | 한국투자증권 OpenAPI 기반 계좌·주문·시세 연동 |
| [yquant-gateway-kisweb](https://github.com/yoonbae81/yquant-gateway-kisweb) | 한국투자증권 웹 기반 연금 계좌 잔고 조회와 주문 자동화 |
| [yquant-gateway-upb](https://github.com/yoonbae81/yquant-gateway-upb) | Upbit 기반 계좌·주문·시세 연동 |
| [yquant-gateway-toss](https://github.com/yoonbae81/yquant-gateway-toss) | Toss 기반 계좌·주문·시세 연동 |

실행 계층은 전략 판단과 브로커 동작의 분리에 기반함

브로커별 서비스는 브로커 인증, 주문 실행, 계좌 동기화, 실시간 상태 수집을 담당함

중앙 Gateway는 공통 상태 계약과 내구성 있는 주문·체결 기록을 담당함

### Operations and platform

| Repository | Focus |
| --- | --- |
| [yquant-console](https://github.com/yoonbae81/yquant-console) | 계좌, 보유종목, 시세, 주문, 예약 주문, 유니버스, 서비스 상태를 다루는 운영 UI |
| [yquant-infra](https://github.com/yoonbae81/yquant-infra) | PostgreSQL·Valkey 인프라, 운영 스크립트, 배포 절차, 프로토콜 거버넌스 |

Console은 운영 요청과 상태 관찰의 접점으로 동작함

Infra는 데이터 소유권, 메시지 형식, 배포 순서, 서비스 간 변경 절차의 기준점으로 동작함

## Private repositories and active development

일부 저장소의 **Private** 상태는 비활성화나 폐기가 아닌 활발한 개발 진행 상태를 의미함

주요 배경

- 브로커 인증 정보, 계좌 구조, 운영 환경 설정 등 공개에 적합하지 않은 구현 세부사항을 포함함
- 서비스 간 계약 변경과 데이터 모델 정비를 지속함
- 실거래 연결 기능의 안전성, 재현성, 운영 절차 검증을 진행함
- 문서, 테스트, 배포 경로, 공개 범위를 정리함
- 안정적인 공개 단위가 마련된 구성요소부터 공개 대상으로 관리함

Private 저장소 링크는 권한 보유자에게 접근을 제공함

외부 공개 범위는 개발 성숙도, 보안 검토, 운영 안정성, 문서화 수준에 따라 결정함

## Scope and disclaimer

yQuant는 개인 연구와 운용 보조를 목적으로 함

- 투자 자문, 수익 보장, 매수·매도 추천 서비스로 제공하지 않음
- 백테스트 결과는 미래 성과를 보장하지 않음
- 자동화 기능 사용 전 브로커 정책, 수수료, 세금, 유동성, 슬리피지, 장애 가능성, 관련 법규 검토가 필요함
- 실제 주문 실행 전 사용자 책임의 설정 검토와 단계적 검증이 필요함
