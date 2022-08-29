import './editor-data-type.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../mocks/data/data-type.data';

import type { UmbEditorDataTypeElement } from './editor-data-type.element';

export default {
	title: 'Editors/Data Type',
	component: 'umb-editor-data-type',
	id: 'umb-editor-data-type',
} as Meta;

export const AAAOverview: Story<UmbEditorDataTypeElement> = () =>
	html` <umb-editor-data-type id="${data[0].id}"></umb-editor-data-type>`;
AAAOverview.storyName = 'Overview';
