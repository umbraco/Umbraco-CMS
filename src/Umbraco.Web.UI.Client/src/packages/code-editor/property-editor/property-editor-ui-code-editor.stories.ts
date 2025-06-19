import type { UmbPropertyEditorUICodeEditorElement } from './property-editor-ui-code-editor.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-code-editor.element.js';

const meta: Meta<UmbPropertyEditorUICodeEditorElement> = {
	title: 'Property Editor UIs/Code Editor',
	component: 'umb-property-editor-ui-code-editor',
	id: 'umb-property-editor-ui-code-editor',
	decorators: [(story) => html`<div style="--editor-height: 400px">${story()}</div>`],
};

export default meta;
type Story = StoryObj<UmbPropertyEditorUICodeEditorElement>;

export const Overview: Story = {};
