import argparse
import re
import sys
from pathlib import Path


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


def update_csproj(csproj_path: Path, new_version: str):
    content = csproj_path.read_text(encoding="utf-8")

    version_pattern = r"<Version>.*?</Version>"
    av_pattern = r"<AssemblyVersion>.*?</AssemblyVersion>"
    fv_pattern = r"<FileVersion>.*?</FileVersion>"

    content = re.sub(version_pattern, f"<Version>{new_version}</Version>", content)
    content = re.sub(
        av_pattern, f"<AssemblyVersion>{new_version}.0</AssemblyVersion>", content
    )
    content = re.sub(fv_pattern, f"<FileVersion>{new_version}.0</FileVersion>", content)

    csproj_path.write_text(content, encoding="utf-8")
    print(f"Updated version to {new_version}")


def update_readme(readme_path: Path, new_version: str):
    content = readme_path.read_text(encoding="utf-8")
    content = re.sub(r"Version \d+\.\d+\.\d+", f"Version {new_version}", content)
    readme_path.write_text(content, encoding="utf-8")
    print(f"Updated README to {new_version}")


def main():
    parser = argparse.ArgumentParser(description="Bump version in csproj")
    parser.add_argument("--version", "-v", help="Version string (x.y.z format)")
    args = parser.parse_args()

    new_version = bump_version(args.version)

    csproj_path = Path(__file__).parent / "WangyiMCCheckworld.csproj"
    readme_path = Path(__file__).parent / "README.md"

    if not csproj_path.exists():
        print("Error: WangyiMCCheckworld.csproj not found")
        sys.exit(1)

    update_csproj(csproj_path, new_version)

    if readme_path.exists():
        update_readme(readme_path, new_version)

    print(f"Done! Version: {new_version}")


if __name__ == "__main__":
    main()
