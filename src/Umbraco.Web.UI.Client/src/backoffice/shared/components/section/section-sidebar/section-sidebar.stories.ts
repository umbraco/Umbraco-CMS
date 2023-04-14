import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbSectionSidebarElement } from './section-sidebar.element';
import './section-sidebar.element';

export default {
	title: 'Sections/Shared/Section Sidebar',
	component: 'umb-section-sidebar',
	id: 'umb-section-sidebar',
} as Meta;

export const AAAOverview: Story<UmbSectionSidebarElement> = () =>
	html` <umb-section-sidebar>Section Sidebar Area</umb-section-sidebar> `;
AAAOverview.storyName = 'Overview';
