import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUICheckboxListElement } from './property-editor-ui-checkbox-list.element';
import './property-editor-ui-checkbox-list.element';

export default {
	title: 'Property Editor UIs/Checkbox List',
	component: 'umb-property-editor-ui-checkbox-list',
	id: 'umb-property-editor-ui-checkbox-list',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICheckboxListElement> = () =>
	html`<umb-property-editor-ui-checkbox-list></umb-property-editor-ui-checkbox-list>`;
AAAOverview.storyName = 'Overview';
