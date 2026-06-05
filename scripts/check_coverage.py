#!/usr/bin/env python3
"""Validate coverage thresholds from a Cobertura XML report."""

from __future__ import annotations

import argparse
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

DEFAULT_MIN_LINE_RATE = 0.50
DEFAULT_MIN_BRANCH_RATE = 0.30


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate line and branch coverage thresholds")
    parser.add_argument(
        "--coverage-file",
        type=Path,
        default=Path("TestResults") / "coverage.cobertura.xml",
        help="Path to the Cobertura XML report to inspect",
    )
    parser.add_argument(
        "--min-line-rate",
        type=float,
        default=DEFAULT_MIN_LINE_RATE,
        help="Minimum required line coverage rate (for example 0.60)",
    )
    parser.add_argument(
        "--min-branch-rate",
        type=float,
        default=DEFAULT_MIN_BRANCH_RATE,
        help="Minimum required branch coverage rate (for example 0.40)",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()

    coverage_file = args.coverage_file
    if not coverage_file.exists():
        candidates = list(Path("TestResults").rglob("coverage.cobertura.xml"))
        if candidates:
            coverage_file = candidates[0]
        else:
            print(f"Coverage file not found: {coverage_file}", file=sys.stderr)
            return 1

    try:
        root = ET.parse(coverage_file).getroot()
    except ET.ParseError as exc:
        print(f"Unable to parse coverage report '{coverage_file}': {exc}", file=sys.stderr)
        return 1

    line_rate = float(root.get("line-rate", 0))
    branch_rate = float(root.get("branch-rate", 0))

    print(f"Coverage file: {coverage_file}")
    print(f"Line coverage: {line_rate:.2%}")
    print(f"Branch coverage: {branch_rate:.2%}")

    if line_rate < args.min_line_rate or branch_rate < args.min_branch_rate:
        print(
            "Coverage threshold not met: "
            f"line={line_rate:.2%} (required >= {args.min_line_rate:.0%}), "
            f"branch={branch_rate:.2%} (required >= {args.min_branch_rate:.0%})",
            file=sys.stderr,
        )
        return 1

    print("Coverage thresholds satisfied.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
