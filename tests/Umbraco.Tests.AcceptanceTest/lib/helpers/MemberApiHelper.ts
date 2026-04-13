import {ApiHelpers} from "./ApiHelpers";
import {MemberBuilder} from "../builders";

export class MemberApiHelper {
  api: ApiHelpers;

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/member/' + id);
    return await response.json();
  }

  async create(member) {
    if (member == null) {
      return;
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/member', member);
    return response.headers().location.split("v1/member/").pop();
  }

  // Read-modify-write update. Pass only the fields you want to change in `overrides`;
  // everything else is preserved from the current server-side state.
  async update(id: string, overrides: Partial<{isApproved: boolean; isLockedOut: boolean; isTwoFactorEnabled: boolean; email: string; username: string}> = {}) {
    const data = await this.get(id);
    const payload = {
      variants: data.variants,
      values: data.values,
      email: data.email,
      username: data.username,
      isApproved: data.isApproved,
      isLockedOut: data.isLockedOut,
      isTwoFactorEnabled: data.isTwoFactorEnabled,
      groups: (data.groups ?? []).map((g: any) => typeof g === 'string' ? g : g.id),
      ...overrides,
    };
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/member/' + id, payload);
  }

  async setLockedOut(id: string, isLockedOut: boolean) {
    return await this.update(id, {isLockedOut});
  }

  async delete(id: string) {
    if (id == null) {
      return;
    }
    const response = await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/member/' + id);
    return response.status();
  }

  async getAll() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/filter/member?skip=0&take=1000');
  }

  async filterByMemberTypeId(memberTypeId: string) {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/filter/member?memberTypeId='+ memberTypeId + '&orderBy=username&skip=0&take=100');
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/member/' + id);
    return response.status() === 200;
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  async getByName(name: string) {
    const rootMembers = await this.getAll();
    const jsonMembers = await rootMembers.json();

    for (const member of jsonMembers.items) {
      if (member.variants[0].name === name) {
        return await this.get(member.id);
      }
    }
    return false;
  }

  async ensureNameNotExists(name: string) {
    const rootMembers = await this.getAll();
    const jsonMembers = await rootMembers.json();
    
    for (const member of jsonMembers.items) {       
      if (member.variants[0].name === name) {
        return await this.delete(member.id);
      }
    }
    return null;
  }

  async createDefaultMember(memberName: string, memberTypeId: string, email: string, username: string, password: string) {
    await this.ensureNameNotExists(memberName);

    const member = new MemberBuilder()
      .addVariant()
        .withName(memberName)
        .done()
      .withEmail(email)
      .withUsername(username)
      .withPassword(password)
      .withMemberTypeId(memberTypeId)
      .build();
    return await this.create(member);
  }

  // createDefaultMember leaves isApproved at its builder default (false); this variant sets it
  // to true so the member can sign in immediately.
  async createApprovedMember(memberName: string, memberTypeId: string, email: string, username: string, password: string) {
    await this.ensureNameNotExists(memberName);

    const member = new MemberBuilder()
      .addVariant()
        .withName(memberName)
        .done()
      .withEmail(email)
      .withUsername(username)
      .withPassword(password)
      .withMemberTypeId(memberTypeId)
      .withIsApproved(true)
      .build();
    return await this.create(member);
  }

  async createMemberWithMemberGroup(memberName: string, memberTypeId: string, email: string, username: string, password: string, memberGroupId: string) {
    const member = new MemberBuilder()
      .addVariant()
        .withName(memberName)
        .done()
      .withEmail(email)
      .withUsername(username)
      .withPassword(password)
      .withMemberTypeId(memberTypeId)
      .addGroup(memberGroupId)
      .build();
    return await this.create(member);
  }
}