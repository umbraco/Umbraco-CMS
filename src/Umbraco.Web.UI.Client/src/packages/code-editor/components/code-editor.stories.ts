import type { CodeEditorLanguage } from '../models/code-editor.model.js';
import { CodeEditorTheme } from '../models/code-editor.model.js';
import type { UmbCodeEditorElement } from './code-editor.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './code-editor.element.js';

const meta: Meta<UmbCodeEditorElement> = {
	title: 'Components/Code Editor',
	component: 'umb-code-editor',
	decorators: [(story) => html`<div style="--editor-height: 400px">${story()}</div>`],
	parameters: { layout: 'fullscreen' },
	argTypes: {
		theme: {
			control: 'select',
			options: [
				CodeEditorTheme.Dark,
				CodeEditorTheme.Light,
				CodeEditorTheme.HighContrastLight,
				CodeEditorTheme.HighContrastLight,
			],
		},
	},
};

const codeSnippets: Record<CodeEditorLanguage, string> = {
	csharp: `using System;

namespace HelloWorld;

public class Program
{
	public static void Main()
	{
		Console.WriteLine("Hello World");
	}
}`,
	javascript: `// Returns "banana"
('b' + 'a' + + 'a' + 'a').toLowerCase();`,
	css: `:host {
	display: flex;
	background-color: var(--uui-color-background);
	width: 100%;
	height: 100%;
	flex-direction: column;
}

#header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	width: 100%;
	height: 70px;
	background-color: var(--uui-color-surface);
	border-bottom: 1px solid var(--uui-color-border);
	box-sizing: border-box;
}

#headline {
	display: block;
	margin: 0 var(--uui-size-layout-1);
}

#tabs {
	margin-left: auto;
}`,
	html: `<!DOCTYPE html>
<html>
<head>
<title>Page Title</title>
</head>
<body>

<h1>This is a Heading</h1>
<p>This is a paragraph.</p>

</body>
</html>`,
	razor: `@using Umbraco.Extensions
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridItem>
@{
	if (Model?.Areas.Any() != true) { return; }
}

<div class="umb-block-grid__area-container"
		style="--umb-block-grid--area-grid-columns: @(Model.AreaGridColumns?.ToString() ?? Model.GridColumns?.ToString() ?? "12");">
	@foreach (var area in Model.Areas)
	{
		@await Html.GetBlockGridItemAreaHtmlAsync(area)
	}
</div>`,
	markdown: `
You will like those projects!

---

# h1 Heading 8-)
## h2 Heading
### h3 Heading
#### h4 Heading
##### h5 Heading
###### h6 Heading


## Horizontal Rules

___

---

***


## Typographic replacements

Enable typographer option to see result.

(c) (C) (r) (R) (tm) (TM) (p) (P) +-

test.. test... test..... test?..... test!....

!!!!!! ???? ,,  -- ---

"Smartypants, double quotes" and 'single quotes'`,
	typescript: `import { UmbTemplateRepository } from '../repository/template.repository.js';
import { UmbWorkspaceContextBase } from '../../../shared/components/workspace/workspace-context/workspace-context.js';
import { UmbObjectState } from '@umbraco-cms/observable-api';
import { TemplateModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/controller';

export class UmbTemplateWorkspaceContext extends UmbWorkspaceContext<UmbTemplateRepository, TemplateModel> {
	#data = new UmbObjectState<TemplateModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.asObservablePart((data) => data?.name);
	content = this.#data.asObservablePart((data) => data?.content);

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.Template', new UmbTemplateRepository(host));
	}

	getData() {
		return this.#data.getValue();
	}

	setName(value: string) {
		this.#data.setValue({ ...this.#data.value, name: value });
	}

	setContent(value: string) {
		this.#data.setValue({ ...this.#data.value, content: value });
	}

	async load(entityId: string) {
		const { data } = await this.repository.requestByKey(entityId);
		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	async createScaffold(parentId: string | null) {
		const { data } = await this.repository.createScaffold(parentId);
		if (!data) return;
		this.setIsNew(true);
		this.#data.setValue(data);
	}
}`,
	json: `{
	"compilerOptions": {
		"module": "esnext",
		"target": "esnext",
		"lib": ["es2020", "dom", "dom.iterable"],
		"declaration": true,
		"emitDeclarationOnly": true,
		"noEmitOnError": true,
		"outDir": "./types",
		"strict": true,
		"noImplicitReturns": true,
		"noFallthroughCasesInSwitch": true,
		"moduleResolution": "node",
		"isolatedModules": true,
		"allowSyntheticDefaultImports": true,
		"experimentalDecorators": true,
		"forceConsistentCasingInFileNames": true,
		"useDefineForClassFields": false,
		"skipLibCheck": true,
		"resolveJsonModule": true,
		"baseUrl": ".",
		"paths": {
			"@umbraco-cms/css": ["libs/css/custom-properties.css"],
			"@umbraco-cms/modal": ["src/core/modal"],
			"@umbraco-cms/models": ["libs/models"],
			"@umbraco-cms/backend-api": ["libs/backend-api"],
			"@umbraco-cms/context-api": ["libs/context-api"],
			"@umbraco-cms/controller": ["libs/controller"],
			"@umbraco-cms/element": ["libs/element"],
			"@umbraco-cms/extension-api": ["libs/extension-api"],
			"@umbraco-cms/extension-registry": ["libs/extension-registry"],
			"@umbraco-cms/notification": ["libs/notification"],
			"@umbraco-cms/observable-api": ["libs/observable-api"],
			"@umbraco-cms/events": ["libs/events"],
			"@umbraco-cms/entity-action": ["libs/entity-action"],
			"@umbraco-cms/workspace": ["libs/workspace"],
			"@umbraco-cms/utils": ["libs/utils"],
			"@umbraco-cms/router": ["libs/router"],
			"@umbraco-cms/sorter": ["libs/sorter"],
			"@umbraco-cms/test-utils": ["libs/test-utils"],
			"@umbraco-cms/repository": ["libs/repository"],
			"@umbraco-cms/resources": ["libs/resources"],
			"@umbraco-cms/store": ["libs/store"],
			"@umbraco-cms/components/*": ["src/backoffice/components/*"],
			"@umbraco-cms/sections/*": ["src/backoffice/sections/*"]
		}
	},
	"include": ["src/**/*.ts", "apps/**/*.ts", "libs/**/*.ts", "e2e/**/*.ts"],
	"references": [
		{
			"path": "./tsconfig.node.json"
		}
	]
}`,
};

export default meta;
type Story = StoryObj<UmbCodeEditorElement>;

const [Csharp, Javascript, Css, Html, Razor, Markdown, Typescript, Json]: Story[] = Object.keys(codeSnippets).map(
	(language) => {
		return {
			args: {
				language: language as CodeEditorLanguage,
				code: codeSnippets[language as CodeEditorLanguage],
			},
		};
	},
);

const Themes: Story = {
	args: {
		language: 'javascript',
		code: codeSnippets.javascript,
		theme: CodeEditorTheme.Dark,
	},
};

export { Csharp, Javascript, Css, Html, Razor, Markdown, Typescript, Json, Themes };
