import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUICollectionViewElement } from './property-editor-ui-collection-view.element';
import './property-editor-ui-collection-view.element';

export default {
	title: 'Property Editor UIs/Collection View',
	component: 'umb-property-editor-ui-collection-view',
	id: 'umb-property-editor-ui-collection-view',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICollectionViewElement> = () =>
	html`<umb-property-editor-ui-collection-view></umb-property-editor-ui-collection-view>`;
AAAOverview.storyName = 'Overview';
