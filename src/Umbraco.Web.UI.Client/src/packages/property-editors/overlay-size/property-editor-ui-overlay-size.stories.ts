import type { UmbPropertyEditorUIOverlaySizeElement } from './property-editor-ui-overlay-size.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-overlay-size.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Overlay Size',
	component: 'umb-property-editor-ui-overlay-size',
	id: 'umb-property-editor-ui-overlay-size',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIOverlaySizeElement> = () =>
	html`<umb-property-editor-ui-overlay-size></umb-property-editor-ui-overlay-size>`;
