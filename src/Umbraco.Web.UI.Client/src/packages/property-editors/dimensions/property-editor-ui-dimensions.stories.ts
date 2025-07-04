import type { UmbPropertyEditorUIDimensionsElement } from './property-editor-ui-dimensions.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-dimensions.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Dimensions',
	component: 'umb-property-editor-ui-dimensions',
	id: 'umb-property-editor-ui-dimensions',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIDimensionsElement> = () =>
	html` <umb-property-editor-ui-dimensions></umb-property-editor-ui-dimensions>`;
