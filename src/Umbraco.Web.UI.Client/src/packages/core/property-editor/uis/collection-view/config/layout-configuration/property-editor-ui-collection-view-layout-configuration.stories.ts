import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUICollectionViewLayoutConfigurationElement } from './property-editor-ui-collection-view-layout-configuration.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-collection-view-layout-configuration.element.js';

export default {
	title: 'Property Editor UIs/Collection View Layout Configuration',
	component: 'umb-property-editor-ui-collection-view-layout-configuration',
	id: 'umb-property-editor-ui-collection-view-layout-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICollectionViewLayoutConfigurationElement> = () =>
	html`<umb-property-editor-ui-collection-view-layout-configuration></umb-property-editor-ui-collection-view-layout-configuration>`;
AAAOverview.storyName = 'Overview';
