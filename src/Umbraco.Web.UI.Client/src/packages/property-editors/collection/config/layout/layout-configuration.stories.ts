import type { UmbPropertyEditorUICollectionLayoutConfigurationElement } from './layout-configuration.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './layout-configuration.element.js';

export default {
	title: 'Property Editor UIs/Collection Layout Configuration',
	component: 'umb-property-editor-ui-collection-layout-configuration',
	id: 'umb-property-editor-ui-collection-layout-configuration',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUICollectionLayoutConfigurationElement> = () =>
	html`<umb-property-editor-ui-collection-layout-configuration></umb-property-editor-ui-collection-layout-configuration>`;
AAAOverview.storyName = 'Overview';
