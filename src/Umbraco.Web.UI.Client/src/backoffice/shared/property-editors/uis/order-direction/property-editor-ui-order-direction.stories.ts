import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIOrderDirectionElement } from './property-editor-ui-order-direction.element';
import './property-editor-ui-order-direction.element';

export default {
	title: 'Property Editor UIs/Order Direction',
	component: 'umb-property-editor-ui-order-direction',
	id: 'umb-property-editor-ui-order-direction',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIOrderDirectionElement> = () =>
	html`<umb-property-editor-ui-order-direction></umb-property-editor-ui-order-direction>`;
AAAOverview.storyName = 'Overview';
