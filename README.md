# Aprimo Labs Public LInks Webcrawler

**Disclaimer: This is an experimental utility and may not work in all scenarios, such as when dealing with a lot of dynamic content loaded via JavaScript or other edge cases.**

## Overview

This WebCrawler Utility is designed to crawl a specified domain and identify pages containing images, videos, and anchor links that match a specified public links domain. It exports the results to a CSV file, including the page URL, page title, item type, and item URL.

## Features

- Crawls a specified domain and identifies images, videos, and anchor links served from an Aprimo Public Link.
- Supports custom public links domains.
- Prevents cyclical links to avoid infinite loops.
- Adds a delay between requests to avoid overloading the target site.
- Exports results to a CSV file with columns for page URL, page title, item type, and item URL.

## Usage

Follow the prompts:

- Enter the domain you want to crawl (without https://). 
- Specify if you are using a custom domain for public links (yes/no).
- If yes, enter the custom domain for public links.

## Results

The results will be exported to a file named crawl_results.csv in the same directory.

The CSV file will have the following columns:

- PageUrl: The URL of the page where the item was found.
- PageTitle: The title of the page.
- ItemType: The type of item (Image, Video, Anchor).
- ItemUrl: The URL of the item.

## Limitations

- Dynamic Content: The utility may not work well with sites that load a lot of content dynamically via JavaScript.
- Single Page Applications (SPAs): SPAs often load content dynamically without changing the URL, which this utility will not handle properly.
- Infinite Scrolling: Pages with infinite scrolling will not be fully crawled since the utility only processes the initially loaded content.
- CAPTCHAs and Rate Limiting: Some websites use CAPTCHAs or rate limiting to prevent automated crawling.
- Login/Authentication Required: The utility cannot crawl pages that require login or authentication.
- Frame and iFrame Content: Content within <frame> or <iframe> tags is not crawled by this utility.
- WebSocket or AJAX-Loaded Data: Content loaded via WebSockets or AJAX calls after the initial page load will not be captured.
- Obfuscated or Minified HTML: Pages with heavily obfuscated or minified HTML may cause parsing issues.
- Sites with Strict Robots.txt: Websites that disallow crawling in their robots.txt file may legally block this utility from accessing certain pages.
- Performance and Resource Limitations: Crawling very large sites can be resource-intensive and may lead to performance issues.