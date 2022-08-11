import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { UmbEditorDataTypeElement } from './editor-data-type.element';
import './editor-data-type.element';

import { data } from '../../../mocks/data/data-type.data';

export default {
	title: 'Editors/Data Type',
	component: 'umb-editor-data-type',
	id: 'umb-editor-data-type',
} as Meta;

export const AAAOverview: Story<UmbEditorDataTypeElement> = () =>
	html` <umb-editor-data-type id="${data[0].id}"></umb-editor-data-type>`;
AAAOverview.storyName = 'Overview';
