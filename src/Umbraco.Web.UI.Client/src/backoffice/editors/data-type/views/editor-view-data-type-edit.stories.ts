import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { UmbDataTypeContext } from '../data-type.context';
import { UmbEditorViewDataTypeEditElement } from './editor-view-data-type-edit.element';
import './editor-view-data-type-edit.element';
import { data } from '../../../../mocks/data/data-type.data';

export default {
	title: 'Editors/Data Type/Views/Edit',
	component: 'umb-editor-view-data-type-edit',
	id: 'umb-editor-view-data-type-edit',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbDataTypeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbEditorViewDataTypeEditElement> = () =>
	html` <umb-editor-view-data-type-edit></umb-editor-view-data-type-edit>`;
AAAOverview.storyName = 'Overview';
