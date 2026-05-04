# Orc.SupportPackage

Orc.SupportPackage is a library that creates support packages of software by gathering relevant information (system info, screenshots, app data, and custom provider data) and bundling it into a ZIP file.

Orc.SupportPackage consists of the following sub-projects:

- `Orc.SupportPackage` => Core library with support package creation functionality.
- `Orc.SupportPackage.Xaml` => WPF library containing XAML-specific views, view models, and services.

---

## Critical Rules (Read First)

These rules are **non-negotiable**. Violating them causes broken builds, crashes, or downstream breakage.

### 1. Never Edit Generated Files

Files matching `*.generated.cs`, `*.generated.xaml` are auto-generated.

- **NEVER** manually edit these files

### 2. ABI / API Stability

This project maintains stable ABI / API. Breaking changes break downstream apps.

| Allowed | Never |
|---------|-------|
| Add new overloads | Modify existing signatures |
| Add new methods | Remove public APIs |
| Add new classes | Change return types |

### 3. Tests Are Mandatory

**Building alone is NOT sufficient.** Run tests before claiming completion (see [Commands](#commands)).

### 4. Branch Protection (COMPLIANCE REQUIRED)

**Direct commits to protected branches are a policy violation.**

| Repository | Protected Branches |
|------------|-------------------|
| Orc.SupportPackage | `master` |
| Orc.SupportPackage | `develop` |

**Required workflow:**

1. **Create a feature branch FIRST** — Use naming convention: `feature/issue-NNNN-description`
2. **Make all commits on the feature branch** — Never commit directly to protected branches
3. **Submit a Pull Request** — Changes must be reviewed by a human before merging

```bash
# CORRECT — Always create a feature branch first
git checkout -b feature/issue-1234-fix-description

# NEVER DO THIS — Policy violation
git checkout develop && git commit  # FORBIDDEN

# NEVER DO THIS — Policy violation
git checkout master && git commit  # FORBIDDEN
```

The repository has protected branches that must be respected.

---

## Commands

Single source of truth for all commands:

| Task | Command |
|------|---------|
| **Build** | `dotnet cake --target=build` |
| **Test** | `dotnet cake --target=test` |
| **Build and test** | `dotnet cake --target=buildandtest` |

---

## Architecture & Directories

### Layer Overview

```
Orc.SupportPackage => Cross-platform core (Windows-targeted: net8.0-windows, net9.0-windows, net10.0-windows)
```

```
Orc.SupportPackage.Xaml => WPF-specific XAML views, view models, and services
```

### Directory Guide

| Directory | Editable? | Notes |
|-----------|-----------|-------|
| `*.generated.cs` | No | Leave as-is |
| `*.generated.xaml` | No | Leave as-is |
| `deployment` | No | Deployment / build scripts |
| `src/Orc.SupportPackage` | Yes | Core support package library |
| `src/Orc.SupportPackage.Xaml` | Yes | WPF-specific components |
| `src/Orc.SupportPackage.Tests` | Yes | Test project |
| `src/Orc.SupportPackage.Example` | Yes | Example WPF application |

---

## Writing Code

### Anti-Patterns (Never Do This)

| Anti-Pattern | Why |
|-------------|-----|
| Modifying method signatures | ABI breaking |
| Manual edits to `*.generated.cs`, `*.generated.xaml` | Overwritten on regenerate |
| Using default parameters in public APIs | ABI breaking |
| **Skipping failing tests** | **Unacceptable — tests must pass** |

---

## Testing & Debugging

### Running Tests

```bash
dotnet cake --target=test
```

### Tests MUST Pass

> **NON-NEGOTIABLE:** Tests must PASS before claiming completion.
>
> - Do NOT skip failing tests
> - Do NOT claim completion if tests fail
> - Do NOT use `SkipException` to work around failures

### Writing Tests

1. Use NUnit to write tests
2. Create a Facts class for a feature
3. Combine Pascal / Snake case for test methods (e.g. `Feature_Does_Work`)

```csharp
[Test]
public void Feature_Does_Work()
{
    var result = 47 - 5;

    Assert.That(result, Is.EqualTo(42));
}
```

### Public API Snapshots

The test project uses `PublicApiGenerator` and `VerifyNUnit` to detect breaking API changes. When a public API change is intentional, update the verified snapshot files:

- `src/Orc.SupportPackage.Tests/PublicApiFacts.Orc_SupportPackage_HasNoBreakingChanges_Async.verified.txt`
- `src/Orc.SupportPackage.Tests/PublicApiFacts.Orc_SupportPackage_Xaml_HasNoBreakingChanges_Async.verified.txt`

**Philosophy:** Tests FAIL when wrong, never skip (except missing hardware).

### Debugging Methodology

1. **Establish baseline** — What's the known-good state?
2. **One change at a time** — Verify each change before proceeding
3. **Track changes in a table** — Log what you changed and the result
4. **Platform differences are signals** — If X works and Y fails, the difference IS the answer
5. **Revert if worse** — Don't pile fixes on top of failures
