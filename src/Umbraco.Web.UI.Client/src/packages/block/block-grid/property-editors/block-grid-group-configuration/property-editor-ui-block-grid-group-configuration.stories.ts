import type { UmbPropertyEditorUIBlockGridGroupConfigurationElement } from './property-editor-ui-block-grid-group-configuration.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-grid-group-configuration.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Block Grid Group Configuration',
	component: 'umb-property-editor-ui-block-grid-group-configuration',
	id: 'umb-property-editor-ui-block-grid-group-configuration',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIBlockGridGroupConfigurationElement> = () =>
	html` <umb-property-editor-ui-block-grid-group-configuration></umb-property-editor-ui-block-grid-group-configuration>`;
