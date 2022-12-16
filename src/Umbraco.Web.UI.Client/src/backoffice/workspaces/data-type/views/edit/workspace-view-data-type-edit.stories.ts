import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../core/mocks/data/data-type.data';
import { UmbDataTypeContext } from '../../data-type.context';

import type { UmbWorkspaceViewDataTypeEditElement } from './workspace-view-data-type-edit.element';

import './workspace-view-data-type-edit.element';

export default {
	title: 'Workspaces/Data Type/Views/Edit',
	component: 'umb-workspace-view-data-type-edit',
	id: 'umb-workspace-view-data-type-edit',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbDataTypeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewDataTypeEditElement> = () =>
	html` <umb-workspace-view-data-type-edit></umb-workspace-view-data-type-edit>`;
AAAOverview.storyName = 'Overview';
