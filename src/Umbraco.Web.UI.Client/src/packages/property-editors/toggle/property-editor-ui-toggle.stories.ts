import type { UmbPropertyEditorUIToggleElement } from './property-editor-ui-toggle.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-toggle.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Toggle',
	component: 'umb-property-editor-ui-toggle',
	id: 'umb-property-editor-ui-toggle',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIToggleElement> = () =>
	html`<umb-property-editor-ui-toggle></umb-property-editor-ui-toggle>`;
