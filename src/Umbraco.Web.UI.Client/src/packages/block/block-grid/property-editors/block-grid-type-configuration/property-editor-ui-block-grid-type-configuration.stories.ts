import type { UmbPropertyEditorUIBlockGridTypeConfigurationElement } from './property-editor-ui-block-grid-type-configuration.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-grid-type-configuration.element.js';

export default {
	title: 'Property Editor UIs/Block Grid Block Configuration',
	component: 'umb-property-editor-ui-block-grid-type-configuration',
	id: 'umb-property-editor-ui-block-grid-type-configuration',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIBlockGridTypeConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-grid-type-configuration></umb-property-editor-ui-block-grid-type-configuration>`;
AAAOverview.storyName = 'Overview';
