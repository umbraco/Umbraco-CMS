import type { UmbPropertyLayoutElement } from './property-layout.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-layout.element.js';

const meta: Meta<UmbPropertyLayoutElement> = {
	title: 'Workspaces/Property Layout',
	id: 'umb-property-layout',
	component: 'umb-property-layout',
	args: {
		alias: 'Umb.PropertyLayout.Text',
		label: 'Text',
		description: 'Description',
		orientation: 'horizontal',
	},
	argTypes: {
		orientation: {
			options: ['horizontal', 'vertical'],
		},
	},
	render: (storyObj) => {
		return html`
			<umb-property-layout
				.invalid=${storyObj.invalid}
				.alias=${storyObj.alias}
				.label=${storyObj.label}
				.description=${storyObj.description}
				.orientation=${storyObj.orientation}>
				<div slot="action-menu"><uui-button color="" look="placeholder">Action Menu</uui-button></div>
				<div slot="editor"><uui-button color="" look="placeholder">Editor</uui-button></div>
			</umb-property-layout>
		`;
	},
	parameters: {
		docs: {
			source: {
				code: `
<umb-property-layout alias="My.Alias" text="My label" description="My description">
	<div slot="action-menu"><uui-button color="" look="placeholder">Action Menu</uui-button></div>
	<div slot="editor"><uui-button color="" look="placeholder">Editor</uui-button></div>
</umb-property-layout>
			`,
			},
		},
	},
};

export default meta;
type Story = StoryObj<UmbPropertyLayoutElement>;

export const Overview: Story = {};

export const Vertical: Story = {
	args: {
		orientation: 'vertical',
	},
	render: (storyObj) => {
		return html`
			<umb-property-layout
				style="width: 500px;"
				.alias=${storyObj.alias}
				.label=${storyObj.label}
				.description=${storyObj.description}
				.orientation=${storyObj.orientation}>
				<uui-textarea slot="editor"></uui-textarea>
			</umb-property-layout>
		`;
	},
};

export const WithInvalid: Story = {
	args: {
		invalid: true,
	},
};

export const WithMarkdown: Story = {
	decorators: [(story) => html` <div style="max-width: 500px; margin:1rem;">${story()}</div> `],
	args: {
		alias: 'Umb.PropertyLayout.Markdown',
		label: 'Markdown',
		description: `
# Markdown
This is a markdown description

## Subtitle
- List item 1
- List item 2

### Sub subtitle
1. Numbered list item 1
2. Numbered list item 2

\`\`\`html
<umb-property-layout>
	<div slot="editor"></div>
</umb-property-layout>

\`\`\`

> Blockquote

**Bold text**

*Italic text*

[Link](https://umbraco.com)

![Image](https://umbraco.com/media/sybjwfmh/umbraco-support-working.webp?anchor=center&mode=crop&width=870&height=628&rnd=133425316954430000&quality=80&format=webp&format=webp)

<details>
<summary>Read more</summary>
Details content
</details>
		`,
	},
};
