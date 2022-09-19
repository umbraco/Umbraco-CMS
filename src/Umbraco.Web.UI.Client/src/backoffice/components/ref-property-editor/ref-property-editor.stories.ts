import './ref-property-editor.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbRefPropertyEditorElement } from './ref-property-editor.element';

export default {
	title: 'Components/Ref Property Editor',
	component: 'umb-ref-property-editor',
	id: 'umb-ref-property-editor',
} as Meta;

export const AAAOverview: Story<UmbRefPropertyEditorElement> = () =>
	html` <umb-ref-property-editor
		name="Custom Property Editor"
		alias="Umb.PropertyEditor.Custom"></umb-ref-property-editor>`;
AAAOverview.storyName = 'Overview';
