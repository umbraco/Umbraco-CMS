import type { UmbPropertyEditorUIBlockListBlockConfigurationElement } from './property-editor-ui-block-list-type-configuration.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-list-type-configuration.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Block List Block Configuration',
	component: 'umb-property-editor-ui-block-list-type-configuration',
	id: 'umb-property-editor-ui-block-list-type-configuration',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIBlockListBlockConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-list-type-configuration></umb-property-editor-ui-block-list-type-configuration>`;
