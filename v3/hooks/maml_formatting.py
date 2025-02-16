# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# NOTES:
# This file implements replacement for MAML headers in markdown using MkDocs native hooks.

import re
import logging
import mkdocs.config
import mkdocs.config.config_options
import mkdocs.plugins
import mkdocs.structure.files
import mkdocs.structure.nav
import mkdocs.structure.pages

log = logging.getLogger(f"mkdocs.plugins.{__name__}")

#
# Hooks
#


def on_page_markdown(markdown: str, page: mkdocs.structure.nav.Page, config: mkdocs.config.Config, files: mkdocs.structure.files.Files) -> str:
    '''Hook on_page_markdown event.'''

    markdown = markdown.replace("## about_PSRule_Assert", "")
    markdown = markdown.replace("## about_PSRule_Baseline", "")
    markdown = markdown.replace("## about_PSRule_Badges", "")
    markdown = markdown.replace("## about_PSRule_Conventions", "")
    markdown = markdown.replace("## about_PSRule_Docs", "")
    markdown = markdown.replace("## about_PSRule_Expressions", "")
    markdown = markdown.replace("## about_PSRule_Keywords", "")
    markdown = markdown.replace("## about_PSRule_Options", "")
    markdown = markdown.replace("## about_PSRule_Rules", "")
    markdown = markdown.replace("## about_PSRule_Selectors", "")
    markdown = markdown.replace("## about_PSRule_SuppressionGroups", "")
    markdown = markdown.replace("## about_PSRule_Variables", "")
    markdown = markdown.replace("# PSRule_Assert", "# Assertion methods")
    markdown = markdown.replace("# PSRule_Baseline", "# Baselines")
    markdown = markdown.replace("# PSRule_Badges", "# Badges")
    markdown = markdown.replace("# PSRule_Conventions", "# Conventions")
    markdown = markdown.replace("# PSRule_Docs", "# Documentation")
    markdown = markdown.replace("# PSRule_Expressions", "# Expressions")
    markdown = markdown.replace("# PSRule_Keywords", "# Keywords")
    markdown = markdown.replace("# PSRule_Options", "# Options")
    markdown = markdown.replace("# PSRule_Rules", "# Rules")
    markdown = markdown.replace("# PSRule_Selectors", "# Selectors")
    markdown = markdown.replace(
        "# PSRule_SuppressionGroups", "# Suppression Groups")
    markdown = markdown.replace("# PSRule_Variables", "# Variables")

    # Rules
    if page.canonical_url.__contains__("/concepts/PSRule/") or page.canonical_url.__contains__("/keywords/PSRule/") or page.canonical_url.__contains__("/commands/PSRule/"):
        markdown = markdown.replace("## SYNOPSIS", "")
        markdown = markdown.replace("## DESCRIPTION", "## Description")
        markdown = markdown.replace("## RECOMMENDATION", "## Recommendation")
        markdown = markdown.replace("## NOTES", "## Notes")
        markdown = markdown.replace("## EXAMPLES", "## Examples")
        markdown = markdown.replace("## LINKS", "## Links")

    # Conceptual topics
    markdown = markdown.replace("## SHORT DESCRIPTION", "")
    markdown = markdown.replace("## LONG DESCRIPTION", "## Description")
    markdown = re.sub(
        "(\#\#\s+(NOTE|KEYWORDS)\s+(.|\s{1,2}(?!\#))+)", "", markdown)
    markdown = markdown.replace("## SEE ALSO", "## Links")

    if page.meta.get('link_users', 'false') != 'false':
        markdown = re.sub(
            r"\@([\w-]*)", r"[@\g<1>](https://github.com/\g<1>)", markdown)

    return markdown
