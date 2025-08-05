import type { UmbPropertyEditorUIDropdownElement } from './property-editor-ui-dropdown.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-dropdown.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Dropdown',
	component: 'umb-property-editor-ui-dropdown',
	id: 'umb-property-editor-ui-dropdown',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIDropdownElement> = () =>
	html`<umb-property-editor-ui-dropdown></umb-property-editor-ui-dropdown>`;
