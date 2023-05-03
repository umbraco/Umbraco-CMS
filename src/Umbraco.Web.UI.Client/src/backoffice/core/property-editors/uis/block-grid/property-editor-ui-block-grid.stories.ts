import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIBlockGridElement } from './property-editor-ui-block-grid.element';
import './property-editor-ui-block-grid.element';

export default {
	title: 'Property Editor UIs/Block Grid',
	component: 'umb-property-editor-ui-block-grid',
	id: 'umb-property-editor-ui-block-grid',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockGridElement> = () =>
	html`<umb-property-editor-ui-block-grid></umb-property-editor-ui-block-grid>`;
AAAOverview.storyName = 'Overview';
