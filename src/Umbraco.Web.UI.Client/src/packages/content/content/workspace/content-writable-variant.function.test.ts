import { _resolveIsWritableVariant } from './content-writable-variant.function.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbVariantNameWriteGuardManager } from '@umbraco-cms/backoffice/workspace';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

@customElement('test-content-writable-variant-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const property = (unique: string, variesByCulture: boolean, variesBySegment = false) =>
	({ unique, variesByCulture, variesBySegment }) as UmbPropertyTypeModel;

describe('_resolveIsWritableVariant', () => {
	const invariant = UmbVariantId.CreateInvariant();
	const da = new UmbVariantId('da');
	const en = new UmbVariantId('en');
	const cultureOptions = [
		{ culture: 'da', segment: null },
		{ culture: 'en', segment: null },
	];
	const varyingProperty = property('varying', true);
	const sharedProperty = property('shared', false);

	let readOnlyGuard: UmbReadOnlyVariantGuardManager;
	let nameWriteGuard: UmbVariantNameWriteGuardManager;
	let propertyWriteGuard: UmbVariantPropertyGuardManager;

	beforeEach(() => {
		const host = new UmbTestControllerHostElement();
		readOnlyGuard = new UmbReadOnlyVariantGuardManager(host);
		nameWriteGuard = new UmbVariantNameWriteGuardManager(host);
		propertyWriteGuard = new UmbVariantPropertyGuardManager(host);
		nameWriteGuard.fallbackToPermitted();
		propertyWriteGuard.fallbackToPermitted();
	});

	const resolve = (
		variantId: UmbVariantId,
		properties = [varyingProperty, sharedProperty],
		variantOptions: Array<{ culture: string | null; segment: string | null }> = cultureOptions,
	) =>
		_resolveIsWritableVariant({
			variantId,
			variantOptions,
			properties,
			readOnlyGuard,
			nameWriteGuard,
			propertyWriteGuard,
		});

	it('is writable when nothing is restricted', () => {
		expect(resolve(da)).to.be.true;
		expect(resolve(invariant)).to.be.true;
	});

	it('is not writable when the old read-only guard marks the variant read-only, whatever else is permitted', () => {
		readOnlyGuard.addRule({ unique: 'ro', variantId: da });

		expect(resolve(da)).to.be.false;
		expect(resolve(en)).to.be.true;
	});

	it('is writable by the name alone', () => {
		propertyWriteGuard.fallbackToNotPermitted();

		expect(resolve(da)).to.be.true;
	});

	it('is writable by a property that belongs to it alone', () => {
		nameWriteGuard.addRule({ unique: 'deny-name', variantId: da, permitted: false });

		expect(resolve(da)).to.be.true;
	});

	it('is not writable when neither its name nor any of its properties may be written', () => {
		nameWriteGuard.addRule({ unique: 'deny-name', variantId: da, permitted: false });
		propertyWriteGuard.addRule({ unique: 'deny-props', datasetVariantId: da, permitted: false });

		expect(resolve(da)).to.be.false;
		expect(resolve(en)).to.be.true;
	});

	it('does not count shared properties towards a culture of content that varies by culture', () => {
		nameWriteGuard.addRule({ unique: 'deny-name', variantId: da, permitted: false });
		propertyWriteGuard.addRule({
			unique: 'deny-varying',
			propertyType: { unique: varyingProperty.unique },
			datasetVariantId: da,
			permitted: false,
		});

		expect(resolve(da)).to.be.false;
	});

	it('resolves the invariant variant from shared properties, and it has no name when the content varies by culture', () => {
		expect(resolve(invariant)).to.be.true;
		expect(resolve(invariant, [varyingProperty])).to.be.false;
	});

	it('is not writable on the invariant variant when its shared properties are denied', () => {
		propertyWriteGuard.addRule({ unique: 'deny-shared', datasetVariantId: invariant, permitted: false });

		expect(resolve(invariant)).to.be.false;
		expect(resolve(da)).to.be.true;
	});

	it('uses the name of the single invariant variant of content that does not vary', () => {
		const invariantOptions = [{ culture: null, segment: null }];
		propertyWriteGuard.fallbackToNotPermitted();

		expect(resolve(invariant, [sharedProperty], invariantOptions)).to.be.true;

		nameWriteGuard.addRule({ unique: 'deny-name', variantId: invariant, permitted: false });

		expect(resolve(invariant, [sharedProperty], invariantOptions)).to.be.false;
	});

	it('stays writable when only some properties are permitted, as granular property permissions do', () => {
		nameWriteGuard.addRule({ unique: 'deny-name', variantId: da, permitted: false });
		propertyWriteGuard.fallbackToNotPermitted();
		propertyWriteGuard.addRule({
			unique: 'allow-one',
			propertyType: { unique: varyingProperty.unique },
			permitted: true,
		});

		expect(resolve(da)).to.be.true;
	});

	it('reflects a rule that is added later', () => {
		expect(resolve(da)).to.be.true;

		readOnlyGuard.addRule({ unique: 'late', variantId: da });

		expect(resolve(da)).to.be.false;
	});

	it('does not count a segment property towards the culture variant', () => {
		const segmentProperty = property('segmented', true, true);
		nameWriteGuard.addRule({ unique: 'deny-name', variantId: da, permitted: false });

		expect(resolve(da, [segmentProperty])).to.be.false;
		expect(resolve(new UmbVariantId('da', 'seg'), [segmentProperty])).to.be.true;
	});
});
