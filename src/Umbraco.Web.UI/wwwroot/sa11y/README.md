![Sa11y, the accessibility quality assurance tool.](https://ryersondmp.github.io/sa11y/assets/github-banner.png)

<h1 align="center">Sa11y</h1>
<p align="center">Meet Sa11y, the <strong>accessibility quality assurance assistant.</strong> Sa11y is a customizable, framework-agnostic JavaScript plugin. Sa11y works as a simple in-page checker that visually highlights common accessibility and usability issues. Geared towards content authors, Sa11y straightforwardly identifies errors or warnings at the source with a simple tooltip on how to fix them. Sa11y is <strong>not</strong> a comprehensive code analysis tool; it exclusively highlights content issues.</p>

* [Project website](https://sa11y.netlify.app/) 🌐
* [Developer documentation](https://sa11y.netlify.app/developers/) 📓
* [Demo](https://ryersondmp.github.io/sa11y/demo/en/) 🚀
* [Report an issue](https://github.com/ryersondmp/sa11y/issues) 🐜
* [Install the WordPress plugin](https://wordpress.org/plugins/sa11y/) 💻
* [WordPress plugin development repo](https://github.com/ryersondmp/sa11y-wp) 🛠
* [Acknowledgements](https://sa11y.netlify.app/acknowledgements/) 👤

## Contributing
Want to help translate or improve Sa11y? Consider [contributing!](https://github.com/ryersondmp/sa11y/blob/master/CONTRIBUTING.md) Translations may either be contributed back to the repository with a pull request, or translated files can be returned to: [adam.chaboryk@ryerson.ca](mailto:adam.chaboryk)

## Contact
Have a question or any feedback? Email: [adam.chaboryk@ryerson.ca](mailto:adam.chaboryk)

<hr>

## Install
Sa11y is a framework-agnostic JavaScript plugin. It's made with vanilla JavaScript and CSS, and its only dependency is Tippy.js - a highly customizable tooltip library that features a positioning system.

To install on your website, insert Sa11y right before the closing </body> tag. Sa11y consists of four files:

- **sa11y.css** - The main stylesheet. Should be included in the <head> of the document (if possible).
- **lang/en.js** - All text strings and tooltip messages. View [supported languages.](https://sa11y.netlify.app/developers/#languages)
- **sa11y.js** - Contains all logic.
- **(Optional) sa11y-custom-checks.js** - Any custom checks created by you.

### NPM
`npm i sa11y`

### Example installation (modules)
````html
<!-- Stylesheet -->
<link rel="stylesheet" href="css/sa11y.css"/>

<!-- JavaScript -->
<script type="module">
  import { Sa11y, Lang } from '../assets/js/sa11y.esm.js';
  import Sa11yLangEn from '../assets/js/lang/en.js';
  import CustomChecks from '../assets/js/sa11y-custom-checks.esm.js'; // Optional

  // Set translations
  Lang.addI18n(Sa11yLangEn.strings);

  // Instantiate
  const sa11y = new Sa11y({
    customChecks: new CustomChecks, // Optional
    checkRoot: "body",
    readabilityRoot: "main",
  });
</script>
````

### Example installation (regular script)
````html
<!-- Stylesheet -->
<link rel="stylesheet" href="css/sa11y.css"/>

<!-- JavaScript -->
<script src="/dist/js/sa11y.umd.min.js"></script>
<script src="/dist/js/lang/en.umd.js"></script>

<!-- Optional: Custom checks -->
<script src="/dist/js/sa11y-custom-checks.umd.min.js"></script>

<!-- Instantiate -->
<script>
  Sa11y.Lang.addI18n(Sa11yLangEn.strings);
  const sa11y = new Sa11y.Sa11y({
    customChecks: new CustomChecks, // Optional
    checkRoot: "body",
    readabilityRoot: "main",
  });
</script>
````

### Example installation (Typescript)
````typescript
// src/your-script.ts

import { Sa11y, Lang, LangEn } from "sa11y";
import CustomChecks from "./your-custom-checks";
import "sa11y/dist/css/sa11y.css";

Lang.addI18n(LangEn.strings);
const sa11y = new Sa11y({
  customChecks: new CustomChecks, // Optional
  checkRoot: "body",
  readabilityRoot: "main",
});

// -------------------------------------------------------------

// src/your-custom-checks.ts
import { Sa11yCustomChecks } from "sa11y";

export default class CustomChecks extends Sa11yCustomChecks {
  check() {
    /**
     * Add custom checks here. For more details, see:
     * - ./src/js/sa11y-custom-checks.js
     * - https://sa11y.netlify.app/developers/custom-checks/
     */
  }
}
````

### CDN
Please visit [developer documentation](https://sa11y.netlify.app/developers/) for CDN installation instructions.

## Development environment
A light server for development is included. Any change inside `/src` folder files will trigger the build process for the files and will reload the page with the new changes. To use this environment:

1. Clone this repo.
2. Be sure you have node installed and up to date.
3. Execute `npm install`
4. In a terminal execute: `npm run serve`. Then open `http://localhost:8080/docs/demo/en/` in your browser.
