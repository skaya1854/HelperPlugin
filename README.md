# Helper Plugin

Unity Editor용 개발 도구 플러그인입니다. Material Design 스타일의 모던한 UI로 디버깅, 성능 모니터링, 컴포넌트 검사 기능을 제공합니다.

## 설치

### 방법 1: 폴더 복사
`Assets/Plugins/HelperPlugin` 폴더를 프로젝트에 복사합니다.

### 방법 2: UPM (Unity Package Manager)
1. `package.json`이 있는 폴더를 Packages 폴더에 넣거나
2. Git URL로 설치: `https://github.com/your-repo/helper-plugin.git`

## 사용법

### Helper Window 열기
- 메뉴: `Window > Helper > Helper Window`
- 단축키: `Ctrl+Shift+Z`

### 기본 탭
| 탭 | 기능 |
|---|---|
| Test | 커스텀 테스트 버튼 |
| Component | 선택 오브젝트의 컴포넌트 분석 |
| Tag | 태그별 오브젝트 목록 |
| Layer | 레이어별 오브젝트 목록 |
| Viewer | 3D 프리뷰 |
| Debug | 실시간 디버깅 정보 (FPS, 메모리 등) |
| Perf | 성능 모니터링 (지오메트리, 렌더링 통계) |

## 확장 기능

### 커스텀 버튼 추가

`[HelperButton]` 속성을 사용하여 테스트 버튼을 추가할 수 있습니다:

```csharp
using HelperPlugin;

[HelperButtonContainer("My Category")]
public static class MyHelperButtons
{
    [HelperButton("My Button", "default value", Category = "My Category", RequiresPlayMode = true)]
    public static void MyButton(string param)
    {
        Debug.Log($"Button clicked with param: {param}");
    }
}
```

### 커스텀 탭 추가

`IHelperExtension` 인터페이스를 구현하여 커스텀 탭을 추가할 수 있습니다:

```csharp
using HelperPlugin;
using UnityEngine;

public class MyCustomTab : HelperExtensionBase
{
    public override string TabName => "Custom";
    public override Color TabColor => new Color(1f, 0.5f, 0f);
    public override int Order => 100;

    public override void OnGUI()
    {
        GUILayout.Label("My Custom Tab Content");
    }
}
```

### 런타임 버튼 등록

코드에서 동적으로 버튼을 등록할 수 있습니다:

```csharp
HelperButtonRegistry.Register(
    name: "Dynamic Button",
    defaultValue: "param",
    action: (p) => Debug.Log(p),
    category: "Runtime",
    requiresPlayMode: false
);
```

## UI 컴포넌트 사용

플러그인의 UI 컴포넌트를 재사용할 수 있습니다:

```csharp
using HelperPlugin;

// 버튼
if (HelperUIComponents.DrawButton("Click Me", HelperTheme.Primary, 100, 24))
{
    // Handle click
}

// 정보 행
HelperUIComponents.DrawInfoRow("Label", "Value", HelperTheme.Accent, "Tooltip");

// 섹션 헤더
HelperUIComponents.DrawPanelHeader("Section Title", HelperTheme.ModeDebug);

// 구분선
HelperUIComponents.DrawDivider();
```

## 테마 색상

`HelperTheme` 클래스에서 일관된 색상을 사용할 수 있습니다:

- **Surface**: `Surface0`, `Surface1`, `Surface2`, `Surface3`, `Border`
- **Brand**: `Primary`, `Secondary`, `Accent`, `Error`
- **Text**: `TextHigh`, `TextMedium`, `TextLow`
- **State**: `StateActive`, `StatePlaying`, `StateFinished`, `StateIdle`
- **Mode**: `ModeTest`, `ModeComponent`, `ModeTag`, `ModeLayer`, `ModeViewer`, `ModeDebug`, `ModePerformance`

## 요구사항

- Unity 2021.3 이상
- TextMeshPro (UI 탭용)

## 라이선스

MIT License
