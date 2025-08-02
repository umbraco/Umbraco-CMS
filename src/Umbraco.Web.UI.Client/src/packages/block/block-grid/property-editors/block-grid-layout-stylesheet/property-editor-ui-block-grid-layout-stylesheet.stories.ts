import type { UmbPropertyEditorUIBlockGridLayoutStylesheetElement } from './property-editor-ui-block-grid-layout-stylesheet.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-grid-layout-stylesheet.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Block/Block Grid Layout Stylesheet',
	component: 'umb-property-editor-ui-block-grid-layout-stylesheet',
	id: 'umb-property-editor-ui-block-grid-layout-stylesheet',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIBlockGridLayoutStylesheetElement> = () =>
	html` <umb-property-editor-ui-block-grid-layout-stylesheet></umb-property-editor-ui-block-grid-layout-stylesheet>`;
