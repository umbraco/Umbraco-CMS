import './editor-view-data-type-info.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../mocks/data/data-type.data';
import { UmbDataTypeContext } from '../../data-type.context';

import type { UmbEditorViewDataTypeInfoElement } from './editor-view-data-type-info.element';

export default {
	title: 'Editors/Data Type/Views/Info',
	component: 'umb-editor-view-data-type-info',
	id: 'umb-editor-view-data-type-info',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbDataTypeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbEditorViewDataTypeInfoElement> = () =>
	html` <umb-editor-view-data-type-info></umb-editor-view-data-type-info>`;
AAAOverview.storyName = 'Overview';
