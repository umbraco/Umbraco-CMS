import { Meta, StoryObj } from '@storybook/web-components';
import './input-slider.element';
import type { UmbInputSliderElement } from './input-slider.element';

const meta: Meta<UmbInputSliderElement> = {
    title: 'Components/Inputs/Slider',
    component: 'umb-input-slider',
    
};
  
export default meta;
type Story = StoryObj<UmbInputSliderElement>;
  
export const Overview: Story = {
    args: {
        min: 0,
        max: 100,
        step: 10,
        initVal1: 20
    }
};

export const WithRange: Story = {
    args: {
        min: 0,
        max: 100,
        step: 10,
        initVal1: 20,
        initVal2: 80,
        enableRange: true
    }
};

export const WithSmallStep: Story = {
    args: {
        min: 0,
        max: 5,
        step: 1,
        initVal1: 4,
    }
};

export const WithLargeMinMax: Story = {
    args: {
        min: 0,
        max: 100,
        step: 1,
        initVal1: 86
    }
};
