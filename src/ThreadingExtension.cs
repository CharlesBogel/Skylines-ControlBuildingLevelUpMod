﻿/*
    Copyright (c) 2015, Max Stark <max.stark88@web.de> 
        All rights reserved.
    
    This file is part of ControlBuildingLevelUpMod, which is free 
    software: you can redistribute it and/or modify it under the terms 
    of the GNU General Public License as published by the Free 
    Software Foundation, either version 2 of the License, or (at your 
    option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
    General Public License for more details. 
    
    You should have received a copy of the GNU General Public License 
    along with this program; if not, see <http://www.gnu.org/licenses/>.
*/

using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Reflection;
using UnityEngine;

namespace ControlBuildingLevelUpMod {
    public class ThreadingExtension : ThreadingExtensionBase {
    
        //private float sumTimeDeltas = 0.0f;

        private UIPanel panelBuildingInfo  = null;
        private UIPanel panelLevelProgress = null;

        public override void OnCreated(IThreading threading) {
            #if DEBUG 
            Logger.Info("ThreadExtensionBase Created");
            #endif
        }

        public override void OnReleased() {
            #if DEBUG 
            Logger.Info("ThreadExtensionBase Released");
            #endif

            if (this.panelLevelProgress != null) {
                foreach (UIComponent progressBar in this.panelLevelProgress.components) {
                    UILabel progressBarLabel = progressBar.GetComponentInChildren<UILabel>();
                    progressBar.RemoveUIComponent(progressBarLabel);
                    progressBar.eventClick -= this.ProgressBarClick;

                    this.panelBuildingInfo = null;
                    this.panelBuildingInfo = null;

                    #if DEBUG 
                    Logger.Info("Click event handler removed");
                    #endif
                }
            }
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            /*
            sumTimeDeltas += realTimeDelta;
            if (sumTimeDeltas > 0.1f) {
                sumTimeDeltas = 0.0f;

            }
            */

            if (this.panelLevelProgress == null || this.panelBuildingInfo == null) {
                this.initView();
            } else {
                if (this.panelBuildingInfo.isVisible) {
                    this.updateView();
                }
            }
        }

        private void initView() {
            this.panelBuildingInfo  = UIView.Find<UIPanel>("(Library) ZonedBuildingWorldInfoPanel");
            this.panelLevelProgress = UIView.Find<UIPanel>("LevelProgress");

            if (this.panelLevelProgress != null) {
                foreach (UIComponent progressBar in this.panelLevelProgress.components) {
                    progressBar.AddUIComponent<UILabel>();
                    progressBar.eventClick += this.ProgressBarClick;

                    #if DEBUG
                    Logger.Info("Click event handler added");
                    #endif
                }
            }
        }

        private void updateView() {
            Level buildingLockLevel = Buildings.getLockLevel(this.getSelectedBuildingID());

            foreach (UIComponent progressBar in this.panelLevelProgress.components) {
                Level progressBarLevel = this.getProgressBarLevel(progressBar);
                UILabel progressBarLabel = progressBar.GetComponentInChildren<UILabel>();

                float x = progressBar.width / 2 - progressBarLabel.width / 2;
                float y = progressBar.height / 2 - progressBarLabel.height / 2;
                progressBarLabel.relativePosition = new Vector3(x, y);
                
                progressBarLabel.text = "";
                if (buildingLockLevel == progressBarLevel) {
                    progressBarLabel.text = "#"; //"■";
                }
            }
        }

        private void ProgressBarClick(UIComponent progressBar, UIMouseEventParameter eventParam) {
            ushort buildingID = this.getSelectedBuildingID();
            Level buildingLockLevel = Buildings.getLockLevel(buildingID);
            Level progressBarLevel  = this.getProgressBarLevel(progressBar);

            if (buildingLockLevel == Level.None) {
                Buildings.add(buildingID, progressBarLevel);
                #if DEBUG 
                Logger.Info("lock level (" + buildingID + "): " + progressBarLevel);
                #endif

            } else {
                if (buildingLockLevel == progressBarLevel) {
                    Buildings.remove(buildingID);
                    #if DEBUG 
                    Logger.Info("lock level (" + buildingID + "): none");
                    #endif

                } else {
                    Buildings.update(buildingID, progressBarLevel);
                    #if DEBUG
                    Logger.Info("lock level (" + buildingID + "): " + progressBarLevel);
                    #endif
                }
            }
        }

        private Level getProgressBarLevel(UIComponent progressBar) {
            switch (progressBar.name) {
                case "Level1Bar": return Level.Level1;
                case "Level2Bar": return Level.Level2;
                case "Level3Bar": return Level.Level3;
                case "Level4Bar": return Level.Level4;
                case "Level5Bar": return Level.Level5;
                default: return Level.None;
            }
        }

        private ushort getSelectedBuildingID() {
            ZonedBuildingWorldInfoPanel panel = this.panelBuildingInfo.gameObject.
                                                GetComponent<ZonedBuildingWorldInfoPanel>();
            return Convert.ToUInt16(
                    this.getProperty("Index", this.getField("m_InstanceID", panel))
                    .ToString());
        }

        private System.Object getField(String name, System.Object obj) {
            MemberInfo[] members = obj.GetType().GetMembers(BindingFlags.Instance |
                                                            BindingFlags.NonPublic |
                                                            BindingFlags.Public |
                                                            BindingFlags.Static);
            foreach (MemberInfo member in members) {
                if (member.MemberType == MemberTypes.Field) {
                    FieldInfo field = (FieldInfo)member;
                    if (field.Name.Equals(name)) {
                        return field.GetValue(obj);
                    }
                }
            }

            return null;
        }

        private System.Object getProperty(String name, System.Object obj) {
            MemberInfo[] members = obj.GetType().GetMembers(BindingFlags.Instance |
                                                            BindingFlags.NonPublic |
                                                            BindingFlags.Public |
                                                            BindingFlags.Static);
            foreach (MemberInfo member in members) {
                if (member.MemberType == MemberTypes.Property) {
                    PropertyInfo property = (PropertyInfo)member;
                    if (property.Name.Equals(name)) {
                        return property.GetValue(obj, null);
                    }
                }
            }

            return null;
        }
    }
}