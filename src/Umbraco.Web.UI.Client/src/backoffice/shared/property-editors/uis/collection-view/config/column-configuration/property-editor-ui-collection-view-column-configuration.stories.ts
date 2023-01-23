import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUICollectionViewColumnConfigurationElement } from './property-editor-ui-collection-view-column-configuration.element';
import './property-editor-ui-collection-view-column-configuration.element';

export default {
	title: 'Property Editor UIs/Collection View Column Configuration',
	component: 'umb-property-editor-ui-collection-view-column-configuration',
	id: 'umb-property-editor-ui-collection-view-column-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICollectionViewColumnConfigurationElement> = () =>
	html`<umb-property-editor-ui-collection-view-column-configuration></umb-property-editor-ui-collection-view-column-configuration>`;
AAAOverview.storyName = 'Overview';
