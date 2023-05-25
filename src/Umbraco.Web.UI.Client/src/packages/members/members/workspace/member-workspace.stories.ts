import './member-workspace.element.js';

import { Meta, Story } from '@storybook/web-components';

import { data } from '../../../../mocks/data/member.data.js';

import type { UmbMemberWorkspaceElement } from './member-workspace.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Workspaces/Member',
	component: 'umb-member-workspace',
	id: 'umb-member-workspace',
} as Meta;

export const AAAOverview: Story<UmbMemberWorkspaceElement> = () =>
	html` <umb-member-workspace id="${data[0].id}"></umb-member-workspace>`;
AAAOverview.storyName = 'Overview';
