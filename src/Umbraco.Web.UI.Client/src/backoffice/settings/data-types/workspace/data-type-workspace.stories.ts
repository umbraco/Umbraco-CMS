import './data-type-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import { data } from '../../../../core/mocks/data/data-type.data';

import type { UmbDataTypeWorkspaceElement } from './data-type-workspace.element';

export default {
	title: 'Workspaces/Data Type',
	component: 'umb-data-type-workspace',
	id: 'umb-data-type-workspace',
} as Meta;

export const AAAOverview: Story<UmbDataTypeWorkspaceElement> = () =>
	html` <umb-data-type-workspace id="${data[0].key}"></umb-data-type-workspace>`;
AAAOverview.storyName = 'Overview';
