# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# NOTES:
# This file implements dynamic navigation for updates using MkDocs native hooks.

import logging
import re
import semver

from mkdocs.config import Config
from mkdocs.config.defaults import MkDocsConfig
from mkdocs.structure.files import File, Files
from mkdocs.structure.pages import Page
from mkdocs.structure.nav import Section, Navigation, _add_parent_links

log = logging.getLogger(f"mkdocs")

#
# Hooks
#


def on_nav(nav: Navigation, config: Config, files: Files) -> Navigation:
    '''Hook on_nav event.'''
    add_updates_to_nav(nav, config, files)

    return nav


def on_page_markdown(markdown: str, page: Page, config: Config, files: Files) -> str:
    '''Hook on_page_markdown event.'''
    markdown = add_update_version_to_title(markdown, page)

    return markdown

#
# Supporting functions
#


def add_updates_to_nav(nav: Navigation, config: Config, files: Files):
    '''Add updates to the nav.'''

    section: Section = next(
        x for x in nav if x.title == "Updates")

    # Get the list of files that are update pages.
    children = []
    for f in files:
        if not f.is_documentation_page():
            continue

        # Check if the page already exists in section children that are Page.
        if any(isinstance(child, Page) and child.file.src_path == f.src_path for child in section.children):
            continue

        destPath = f._get_dest_path(False)
        if not is_update_page(destPath):
            continue
        children.append(f)

    # Sort by semver version string.
    children.sort(key=lambda x: semver.VersionInfo.parse(
        x.src_path.split('/')[-1].replace(".md", ".0").replace("v", "")), reverse=False)

    # Add the more recent 10 updates to the nav.
    for child in children[:10]:
        log.info(f"Added {child.src_path} to list of updates.")
        section.children.insert(0, Page(None, child, config))

    _add_parent_links(nav)


def add_update_version_to_title(markdown: str, page: Page) -> str:
    '''Add version to title of update pages.'''

    if not is_update_page(page.canonical_url):
        return markdown

    version = page.meta.get('version', None)
    if not version:
        return markdown

    title = re.search(r"^# (.+)$", markdown, flags=re.M).group(1)
    page.title = title

    # Append the version number to the first H1 title of the page
    return markdown.replace(f"# {title}", f"# {title} (version {version})")


def is_update_page(path: str) -> bool:
    return path.__contains__("updates/v")
