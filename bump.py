import argparse
import re
import sys
from pathlib import Path

RESET = "\033[0m"
BOLD = "\033[1m"
GREEN = "\033[92m"
YELLOW = "\033[93m"
BLUE = "\033[94m"
CYAN = "\033[96m"
RED = "\033[91m"


def print_info(msg: str):
    print(f"{BLUE}ℹ{RESET} {msg}")


def print_success(msg: str):
    print(f"{GREEN}✓{RESET} {msg}")


def print_updated(field: str, old: str, new: str):
    print(f"{YELLOW}  ├─{RESET} {field}: {BOLD}{old}{RESET} → {GREEN}{new}{RESET}")


def print_file(filename: str):
    print(f"{CYAN}📄 {filename}{RESET}")


def bump_version(version: str | None) -> str:
    if version is None:
        return "0.0.0"

    parts = version.split(".")
    if len(parts) == 1:
        return f"{parts[0]}.0.0"
    elif len(parts) == 2:
        return f"{parts[0]}.{parts[1]}.0"
    elif len(parts) >= 3:
        return f"{parts[0]}.{parts[1]}.{parts[2]}"
    return "0.0.0"


def get_current_versions(csproj_path: Path) -> dict:
    content = csproj_path.read_text(encoding="utf-8")
    v_match = re.search(r"<Version>(.*?)</Version>", content)
    av_match = re.search(r"<AssemblyVersion>(.*?)</AssemblyVersion>", content)
    fv_match = re.search(r"<FileVersion>(.*?)</FileVersion>", content)
    return {
        "Version": v_match.group(1) if v_match else "N/A",
        "AssemblyVersion": av_match.group(1) if av_match else "N/A",
        "FileVersion": fv_match.group(1) if fv_match else "N/A",
    }


def update_csproj(csproj_path: Path, new_version: str):
    current = get_current_versions(csproj_path)
    print_file("WangyiMCCheckworld.csproj")

    content = csproj_path.read_text(encoding="utf-8")

    version_pattern = r"<Version>.*?</Version>"
    av_pattern = r"<AssemblyVersion>.*?</AssemblyVersion>"
    fv_pattern = r"<FileVersion>.*?</FileVersion>"

    new_av = f"{new_version}.0"
    new_fv = f"{new_version}.0"

    content = re.sub(version_pattern, f"<Version>{new_version}</Version>", content)
    content = re.sub(
        av_pattern, f"<AssemblyVersion>{new_av}</AssemblyVersion>", content
    )
    content = re.sub(fv_pattern, f"<FileVersion>{new_fv}</FileVersion>", content)

    csproj_path.write_text(content, encoding="utf-8")

    print_updated("Version", current["Version"], new_version)
    print_updated("AssemblyVersion", current["AssemblyVersion"], new_av)
    print_updated("FileVersion", current["FileVersion"], new_fv)
    print_success("Updated csproj")


def get_current_readme_version(readme_path: Path) -> str:
    content = readme_path.read_text(encoding="utf-8")
    match = re.search(r"Version (\d+\.\d+\.\d+)", content)
    return match.group(1) if match else "N/A"


def update_readme(readme_path: Path, new_version: str):
    current = get_current_readme_version(readme_path)
    print_file("README.md")

    content = readme_path.read_text(encoding="utf-8")
    content = re.sub(r"Version \d+\.\d+\.\d+", f"Version {new_version}", content)
    readme_path.write_text(content, encoding="utf-8")

    print_updated("Version", current, new_version)
    print_success("Updated README")


def main():
    print(f"\n{BOLD}[Version Bump Tool]{RESET}\n")

    parser = argparse.ArgumentParser(description="Bump version in csproj")
    parser.add_argument("--version", "-v", help="Version string (x.y.z format)")
    args = parser.parse_args()

    new_version = bump_version(args.version)
    print_info(f"Target version: {BOLD}{new_version}{RESET}\n")

    csproj_path = Path(__file__).parent / "WangyiMCCheckworld.csproj"
    readme_path = Path(__file__).parent / "README.md"

    if not csproj_path.exists():
        print(f"{RED}✗ Error: WangyiMCCheckworld.csproj not found{RESET}")
        sys.exit(1)

    print(f"{BOLD}📝 Updating files...{RESET}\n")
    update_csproj(csproj_path, new_version)
    print()

    if readme_path.exists():
        update_readme(readme_path, new_version)
    else:
        print_info("README.md not found, skipping")

    print(f"\n{GREEN}✅ Done! All files updated to v{new_version}{RESET}\n")


if __name__ == "__main__":
    main()
