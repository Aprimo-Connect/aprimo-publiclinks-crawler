# Aprimo Labs Public Links Webcrawler

### Aprimo's Open Source Policy 
This code is provided by Aprimo _as-is_ as an example of how you might solve a specific business problem. It is not intended for direct use in Production without modification.

You are welcome to submit issues or feedback to help us improve visibility into potential bugs or enhancements. Aprimo may, at its discretion, address minor bugs, but does not guarantee fixes or ongoing support.

It is expected that developers who clone or use this code take full responsibility for supporting, maintaining, and securing any deployments derived from it.

If you are interested in a production-ready and supported version of this solution, please contact your Aprimo account representative. They can connect you with our technical services team or a partner who may be able to build and support a packaged implementation for you.

Please note: This code may include references to non-Aprimo services or APIs. You are responsible for acquiring any required credentials or API keys to use those services—Aprimo does not provide them.
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

![image](https://github.com/Aprimo-Connect/aprimo-publiclinks-crawler/assets/37909285/d8ae7712-9558-43ae-8f73-2b4fbe5b850d)


## Limitations

Below are some known limitations of the utility. This may not be a comprehensive list.

- Dynamic Content: The utility may not work well with sites that load a lot of content dynamically via JavaScript.
- Content served for different devices: The utility may only find content that aligns to a single browser experience (i.e. if you serve different content for a mobile experience, that will not reflect).
- Personalized content: The utility will not find content that dynamically adjusts based on visitor personalization.
- Single Page Applications (SPAs): SPAs often load content dynamically without changing the URL, which this utility will not handle properly.
- Infinite Scrolling: Pages with infinite scrolling will not be fully crawled since the utility only processes the initially loaded content.
- CAPTCHAs and Rate Limiting: Some websites use CAPTCHAs or rate limiting to prevent automated crawling.
- Login/Authentication Required: The utility cannot crawl pages that require login or authentication.
- Frame and iFrame Content: Content within <frame> or <iframe> tags is not crawled by this utility.
- WebSocket or AJAX-Loaded Data: Content loaded via WebSockets or AJAX calls after the initial page load will not be captured.
- Obfuscated or Minified HTML: Pages with heavily obfuscated or minified HTML may cause parsing issues.
- Sites with Strict Robots.txt: Websites that disallow crawling in their robots.txt file may legally block this utility from accessing certain pages.

# Open Source Policy

For more information about Aprimo's Open Source Policies, please refer to
https://community.aprimo.com/knowledgecenter/aprimo-connect/aprimo-connect-open-source

